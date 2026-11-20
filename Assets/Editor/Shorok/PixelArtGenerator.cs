using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Shorok.EditorTools
{
    public static class PixelArtGenerator
    {
        public const string Root = "Assets/Game/Art/Generated/";
        public static readonly Color32[] Palette = { G(18), G(34), G(53), G(78), G(110), G(151), G(199), G(244) };
        static Color32 G(byte g) => new Color32(g, g, g, 255);
        public sealed class Canvas
        {
            public readonly int w, h;
            public readonly Color32[] pixels;
            public Canvas(int width, int height) { w = width; h = height; pixels = new Color32[w * h]; }
            public void Dot(int x, int y, int color) { if (x >= 0 && y >= 0 && x < w && y < h) pixels[y * w + x] = Palette[Mathf.Clamp(color, 0, 7)]; }
            public void Rect(int x, int y, int width, int height, int color) { for (int j = y; j < y + height; j++) for (int i = x; i < x + width; i++) Dot(i, j, color); }
            public void Line(int x1, int y1, int x2, int y2, int color)
            {
                int n = Mathf.Max(Mathf.Abs(x2 - x1), Mathf.Abs(y2 - y1));
                for (int i = 0; i <= n; i++) { float t = n == 0 ? 0 : (float)i / n; Dot(Mathf.RoundToInt(Mathf.Lerp(x1, x2, t)), Mathf.RoundToInt(Mathf.Lerp(y1, y2, t)), color); }
            }
            public void Oval(int x, int y, int rx, int ry, int color)
            {
                for (int j = -ry; j <= ry; j++) for (int i = -rx; i <= rx; i++) if ((float)i * i / (rx * rx) + (float)j * j / (ry * ry) <= 1) Dot(x + i, y + j, color);
            }
            public void Frame(int x, int y, int width, int height, int color) { Rect(x, y, width, 1, color); Rect(x, y + height - 1, width, 1, color); Rect(x, y, 1, height, color); Rect(x + width - 1, y, 1, height, color); }
            public Sprite Save(string name)
            {
                Directory.CreateDirectory(Root); var tex = new Texture2D(w, h, TextureFormat.RGBA32, false); tex.SetPixels32(pixels); tex.Apply();
                string path = Root + name + ".png"; File.WriteAllBytes(path, tex.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(tex); return Import(path);
            }
        }
        public static Sprite Import(string path)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var imp = (TextureImporter)AssetImporter.GetAtPath(path); imp.textureType = TextureImporterType.Sprite; imp.spritePixelsPerUnit = 32;
            imp.filterMode = FilterMode.Point; imp.mipmapEnabled = false; imp.wrapMode = TextureWrapMode.Clamp;
            imp.textureCompression = TextureImporterCompression.Uncompressed; imp.alphaIsTransparency = true; imp.isReadable = true;
            imp.SaveAndReimport(); return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        [MenuItem("Tools/Shorok/Process Ghost Sprite")]
        public static void ProcessGhost()
        {
            string source = "Assets/Game/Art/Ghost/Source/ghost_source.png";
            var tex = new Texture2D(2, 2); tex.LoadImage(File.ReadAllBytes(source)); int w = tex.width, h = tex.height;
            var pixels = tex.GetPixels32(); var counts = new Dictionary<uint, int>();
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) if (x == 0 || y == 0 || x == w - 1 || y == h - 1)
            {
                var c = pixels[y * w + x]; uint key = ((uint)c.r << 16) | ((uint)c.g << 8) | c.b;
                counts[key] = counts.TryGetValue(key, out int n) ? n + 1 : 1;
            }
            var edge = new List<KeyValuePair<uint, int>>(counts); edge.Sort((a, b) => b.Value.CompareTo(a.Value));
            bool Background(Color32 c)
            {
                if (c.a < 10) return true;
                for (int i = 0; i < Mathf.Min(2, edge.Count); i++)
                {
                    uint key = edge[i].Key;
                    if (Mathf.Abs(c.r - ((key >> 16) & 255)) <= 5 && Mathf.Abs(c.g - ((key >> 8) & 255)) <= 5 && Mathf.Abs(c.b - (key & 255)) <= 5) return true;
                }
                return false;
            }
            var queue = new Queue<int>(); var visited = new bool[pixels.Length];
            void Add(int x, int y) { if (x < 0 || x >= w || y < 0 || y >= h) return; int n = y * w + x; if (!visited[n] && Background(pixels[n])) { visited[n] = true; queue.Enqueue(n); } }
            for (int x = 0; x < w; x++) { Add(x, 0); Add(x, h - 1); }
            for (int y = 0; y < h; y++) { Add(0, y); Add(w - 1, y); }
            while (queue.Count > 0) { int n = queue.Dequeue(); pixels[n].a = 0; int x = n % w, y = n / w; Add(x - 1, y); Add(x + 1, y); Add(x, y - 1); Add(x, y + 1); }
            int minX = w, maxX = 0, minY = h, maxY = 0;
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) if (pixels[y * w + x].a > 0) { minX = Mathf.Min(minX, x); maxX = Mathf.Max(maxX, x); minY = Mathf.Min(minY, y); maxY = Mathf.Max(maxY, y); }
            string dir = "Assets/Game/Art/Ghost/Generated/"; Directory.CreateDirectory(dir);
            tex.SetPixels32(pixels); tex.Apply(); File.WriteAllBytes(dir + "ghost_full_clean.png", tex.EncodeToPNG());
            int cw = Mathf.CeilToInt((maxX - minX + 1) / 4f) + 2, ch = Mathf.CeilToInt((maxY - minY + 1) / 4f) + 2;
            var clean = new Texture2D(cw, ch, TextureFormat.RGBA32, false); var small = new Color32[cw * ch];
            for (int y = 1; y < ch - 1; y++) for (int x = 1; x < cw - 1; x++) small[y * cw + x] = pixels[Mathf.Min(maxY, minY + (y - 1) * 4 + 1) * w + Mathf.Min(maxX, minX + (x - 1) * 4 + 1)];
            clean.SetPixels32(small); clean.Apply(); File.WriteAllBytes(dir + "ghost_clean.png", clean.EncodeToPNG()); Import(dir + "ghost_clean.png");
            for (int y = ch / 2; y < ch; y++) for (int x = 0; x < cw; x++) if (small[y * cw + x].a > 0 && small[y * cw + x].r < 30) small[y * cw + x] = G(255);
            // The source's two eyes become short pixel eyelids; the outer grey outline is untouched.
            for (int x = 0; x < cw; x++)
            {
                int eyeY = -1;
                for (int y = ch / 2; y < ch; y++) if (clean.GetPixel(x, y).a > .5f && clean.GetPixel(x, y).r < .12f) { eyeY = y; break; }
                if (eyeY >= 0) small[eyeY * cw + x] = G(0);
            }
            clean.SetPixels32(small); clean.Apply(); File.WriteAllBytes(dir + "ghost_blink.png", clean.EncodeToPNG()); Import(dir + "ghost_blink.png");
            UnityEngine.Object.DestroyImmediate(clean); UnityEngine.Object.DestroyImmediate(tex);
        }
        [MenuItem("Tools/Shorok/Generate Pixel Art")]
        public static void Generate()
        {
            Canvas c = new Canvas(1, 1); c.Dot(0, 0, 7); c.Save("pixel");
            c = new Canvas(32, 16); c.Rect(0, 0, 32, 16, 2); c.Rect(0, 13, 32, 3, 5); c.Line(0, 12, 31, 12, 1); c.Line(0, 3, 31, 3, 1); c.Line(7, 5, 23, 5, 3); c.Line(2, 10, 16, 10, 3); c.Dot(3, 14, 1); c.Dot(28, 14, 1); c.Save("plank");
            c = new Canvas(64, 12); c.Rect(0, 2, 64, 10, 1); c.Rect(1, 5, 62, 6, 4); c.Rect(1, 10, 62, 2, 6); c.Line(5, 7, 30, 7, 3); c.Line(38, 6, 57, 6, 2); c.Dot(2, 8, 1); c.Dot(61, 8, 1); c.Save("shelf");
            c = new Canvas(48, 16); c.Rect(1, 0, 46, 16, 1); c.Rect(3, 2, 42, 12, 5); c.Rect(0, 0, 48, 2, 3); c.Rect(0, 14, 48, 2, 6); c.Rect(1, 1, 4, 14, 4); for (int y = 4; y < 14; y += 3) c.Line(8, y, 44, y, 4); c.Save("book");
            c = new Canvas(32, 32); c.Rect(0, 0, 32, 32, 1); c.Rect(2, 2, 28, 28, 4); for (int x = 7; x < 32; x += 8) c.Rect(x, 2, 1, 28, 2); c.Line(2, 2, 29, 29, 6); c.Line(2, 3, 28, 29, 5); c.Frame(2, 2, 28, 28, 6); c.Save("crate");
            c = new Canvas(192, 156);
            c.Rect(5, 1, 13, 12, 1); c.Rect(174, 1, 13, 12, 1);
            c.Rect(4, 9, 184, 141, 1); c.Rect(8, 13, 176, 132, 3);
            for (int row = 0; row < 3; row++)
            {
                int y = 17 + row * 41;
                c.Rect(14, y, 164, 37, 1); c.Rect(17, y + 3, 157, 32, 4);
                c.Line(19, y + 33, 172, y + 33, 5); c.Line(173, y + 5, 173, y + 30, 2);
                for (int k = 0; k < 8; k++) { int gx = 20 + (k * 37 + row * 13) % 134, gy = y + 6 + (k * 7) % 24; c.Line(gx, gy, Mathf.Min(170, gx + 9 + k), gy, k % 3 == 0 ? 5 : 3); }
                for (int x = 49; x < 170; x += 89)
                {
                    c.Oval(x - 9, y + 20, 3, 3, 1); c.Oval(x + 9, y + 20, 3, 3, 1);
                    c.Line(x - 9, y + 20, x - 9, y + 12, 2); c.Line(x + 9, y + 20, x + 9, y + 12, 2);
                    c.Rect(x - 8, y + 10, 16, 3, 2); c.Line(x - 7, y + 13, x + 7, y + 13, 5);
                    c.Dot(x - 10, y + 21, 6); c.Dot(x + 8, y + 21, 6);
                }
            }
            c.Rect(0, 146, 192, 7, 2); c.Rect(2, 151, 188, 4, 5); c.Line(9, 150, 181, 150, 6);
            c.Rect(1, 8, 190, 6, 2); c.Line(5, 13, 186, 13, 5); c.Save("cupboard");
            c = new Canvas(32, 64); c.Oval(16, 4, 14, 4, 1); c.Rect(3, 4, 26, 3, 6); c.Rect(14, 7, 4, 32, 2); c.Rect(15, 8, 2, 32, 6); for (int y = 37; y < 61; y++) { int width = 14 - (y - 37) / 3; c.Rect(16 - width, y, width * 2, 1, y == 37 ? 7 : 5); } c.Line(8, 59, 23, 59, 7); c.Rect(5, 37, 22, 2, 1); c.Save("lamp");
            c = new Canvas(64, 96); for (int y = 0; y < 96; y++) { int width = 30 - y / 4; for (int x = 32 - width; x < 32 + width; x++) if ((x + y) % 4 == 0) c.Dot(x, y, 6); } c.Save("beam");
            c = new Canvas(40, 48); c.Rect(8, 0, 24, 5, 4); c.Rect(18, 3, 4, 16, 6); c.Oval(20, 29, 19, 18, 1); c.Oval(20, 29, 17, 16, 5); c.Oval(20, 29, 14, 13, 2); for (int x = 9; x < 35; x += 5) c.Line(x, 19, x, 39, 4); c.Frame(2, 12, 36, 34, 3); c.Save("fan");
            c = new Canvas(24, 24); c.Oval(12, 12, 3, 3, 7); for (int i = 0; i < 3; i++) { double a = i * Math.PI * 2 / 3; c.Oval(12 + (int)(Math.Cos(a) * 6), 12 + (int)(Math.Sin(a) * 6), 5, 3, 6); } c.Save("blades");
            c = new Canvas(48, 24); c.Rect(2, 6, 44, 9, 1); c.Rect(4, 8, 39, 7, 6); c.Rect(13, 14, 23, 10, 2); c.Rect(16, 16, 7, 6, 5); c.Rect(25, 16, 7, 6, 4); c.Oval(11, 5, 5, 5, 0); c.Oval(37, 5, 5, 5, 0); c.Oval(11, 5, 2, 2, 6); c.Oval(37, 5, 2, 2, 6); c.Rect(42, 11, 4, 3, 7); c.Save("car");
            c = new Canvas(40, 56); c.Rect(4, 0, 32, 48, 1); c.Rect(6, 3, 28, 43, 4); c.Oval(20, 40, 17, 15, 1); c.Oval(20, 40, 14, 12, 6); c.Oval(20, 40, 12, 10, 2); c.Rect(11, 7, 18, 17, 1); c.Line(20, 24, 20, 12, 5); c.Oval(20, 10, 5, 3, 6); c.Dot(20, 49, 7); c.Dot(10, 40, 7); c.Dot(30, 40, 7); c.Save("clock");
            c = new Canvas(20, 20); c.Line(10, 10, 10, 19, 7); c.Line(10, 10, 16, 7, 7); c.Oval(10, 10, 2, 2, 5); c.Save("hand");
            c = new Canvas(32, 40); c.Rect(3, 0, 26, 7, 2); c.Rect(6, 3, 20, 4, 6); c.Rect(10, 7, 12, 13, 4); c.Rect(7, 20, 18, 12, 5); c.Oval(16, 31, 9, 8, 2); c.Oval(15, 32, 7, 6, 5); c.Rect(11, 34, 3, 2, 7); c.Rect(20, 29, 4, 6, 3); c.Save("heavy");
            c = new Canvas(40, 32); c.Rect(0, 0, 40, 20, 1); c.Rect(2, 2, 36, 16, 4); c.Frame(4, 4, 32, 12, 6); c.Rect(17, 8, 6, 4, 2); c.Rect(0, 20, 40, 4, 6); c.Rect(3, 23, 34, 8, 2); c.Frame(5, 24, 30, 6, 5); c.Save("music");
            c = new Canvas(16, 20); c.Line(5, 3, 5, 17, 7); c.Line(5, 17, 13, 15, 7); c.Oval(3, 3, 3, 2, 6); c.Save("note");
            c = new Canvas(16, 20); for (int y = 2; y < 18; y++) { int width = y < 10 ? (y - 1) / 2 : (18 - y) / 2; c.Rect(8 - width, y, width * 2 + 1, 1, 5); } c.Line(8, 3, 8, 16, 7); c.Line(5, 10, 10, 14, 7); c.Save("memory");
            c = new Canvas(64, 16); c.Rect(0, 0, 64, 16, 1); c.Rect(2, 2, 60, 12, 4); for (int x = 5; x < 62; x += 6) c.Rect(x, 4, 3, 8, 1); c.Frame(1, 1, 62, 14, 6); c.Save("vent");
            c = new Canvas(32, 8); c.Rect(0, 0, 32, 5, 2); c.Rect(3, 4, 26, 3, 6); c.Line(7, 5, 13, 5, 1); c.Line(20, 5, 26, 5, 1); c.Save("plate");
            c = new Canvas(32, 96); c.Rect(1, 0, 30, 96, 1); c.Rect(3, 0, 26, 95, 4); for (int y = 4; y < 96; y += 12) { c.Rect(5, y, 22, 4, 2); c.Line(5, y + 5, 26, y + 5, 6); } c.Rect(0, 92, 32, 4, 6); c.Save("gate");
            c = new Canvas(48, 48); c.Oval(24, 24, 21, 21, 3); for (int i = 0; i < 12; i++) { double a = i * Math.PI / 6; c.Rect(21 + (int)(Math.Cos(a) * 21), 21 + (int)(Math.Sin(a) * 21), 6, 6, 5); } c.Oval(24, 24, 15, 15, 1); c.Oval(24, 24, 8, 8, 6); c.Oval(24, 24, 4, 4, 1); c.Save("gear");
            c = new Canvas(32, 64); for (int y = 0; y < 60; y++) for (int x = 3; x < 30; x++) if ((x + y / 3) % 7 < 2 && x > y / 8 && x < 32 - y / 8) c.Dot(x, y, y % 8 < 4 ? 6 : 4); c.Save("steam");
            c = new Canvas(64, 48); for (int y = 4; y < 42; y++) for (int x = 2; x < 62; x++) if (Mathf.Pow((x - 32) / 30f, 2) + Mathf.Pow((y - 23) / 21f, 2) < 1 && (x + y * 3) % 13 > 1) c.Dot(x, y, (x + y) % 7 == 0 ? 2 : 0); c.Save("shadow");
            c = new Canvas(96, 160); c.Rect(8, 0, 80, 137, 2); c.Oval(48, 128, 40, 30, 2); c.Rect(13, 5, 70, 128, 4); c.Oval(48, 127, 35, 26, 4); c.Oval(48, 123, 30, 27, 1); c.Oval(48, 123, 26, 23, 6); for (int i = 0; i < 12; i++) { double a = i * Math.PI / 6; c.Rect(47 + (int)(Math.Cos(a) * 22), 122 + (int)(Math.Sin(a) * 19), 2, 3, 2); } c.Rect(24, 20, 48, 69, 1); c.Frame(23, 19, 50, 71, 5); c.Rect(46, 40, 4, 45, 4); c.Oval(48, 40, 13, 10, 6); c.Rect(3, 0, 90, 8, 5); c.Save("grandclock");
            AssetDatabase.SaveAssets();
        }
    }
}
