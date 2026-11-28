using UnityEngine;
using Screen = LoopDelivery.Giulio.DistrictOneSession.Screen;

namespace LoopDelivery.Giulio
{
    [RequireComponent(typeof(DistrictOneSession))]
    public sealed class DistrictOneHud : MonoBehaviour
    {
        private static readonly Color Ink = new Color32(28, 43, 48, 255);
        private static readonly Color Dark = new Color32(27, 47, 51, 250);
        private static readonly Color Cream = new Color32(249, 239, 214, 255);
        private static readonly Color Linen = new Color32(231, 217, 188, 255);
        private static readonly Color Gold = new Color32(251, 200, 109, 255);
        private static readonly Color Sea = new Color32(164, 219, 192, 255);
        private static readonly Color Green = new Color32(49, 93, 77, 255);
        private static readonly Color Market = new Color32(229, 163, 137, 255);
        private static readonly Color Muted = new Color32(92, 100, 92, 255);

        private DistrictOneSession session;
        private GUIStyle label;

        private void Awake() => session = GetComponent<DistrictOneSession>();

        private void OnGUI()
        {
            if (Event.current.type != EventType.Repaint)
                return;
            label ??= new GUIStyle(GUI.skin.label)
            {
                padding = new RectOffset(),
                clipping = TextClipping.Clip,
                wordWrap = true
            };

            float scale = Mathf.Min(UnityEngine.Screen.width / 1100f, UnityEngine.Screen.height / 680f);
            Matrix4x4 original = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
            float width = UnityEngine.Screen.width / scale;
            float height = UnityEngine.Screen.height / scale;
            DrawHud(width, height);

            switch (session.ActiveScreen)
            {
                case Screen.Manifest:
                case Screen.Map:
                case Screen.Clues:
                    DrawNotebook(width, height);
                    break;
                case Screen.World:
                    break;
                default:
                    DrawMessage(width, height);
                    break;
            }
            if (Time.unscaledTime < session.NoticeUntil)
            {
                Rect toast = new(width / 2 - 230, 21, 460, 40);
                Card(toast, Dark, Gold);
                Text(toast, session.Notice, 17, Cream, true, TextAnchor.MiddleCenter);
            }
            GUI.matrix = original;
        }

        private void DrawHud(float width, float height)
        {
            Rect manifest = new(20, 20, 281, 99);
            Card(manifest, Dark, Sea);
            Text(new Rect(35, 31, 251, 19), "COURIER MANIFEST", 14, Sea, true);
            Text(new Rect(35, 48, 251, 32), $"{session.SignedCount}/3 delivered", 25, Cream, true);
            Text(new Rect(35, 82, 251, 23), session.Objective, 15, Cream);
            Rect time = new(width - 202, 20, 182, 82);
            Card(time, Dark, Gold);
            Text(new Rect(time.x + 15, 30, 154, 35), DistrictOneSession.FormatTime(session.Now), 29, Cream, true);
            uint now = session.Now;
            var harbour = session.Schedule.CanalUnloading;
            string timer = session.InDepot ? "Depot · clock paused"
                : session.ActiveScreen != Screen.World ? "Clock paused"
                : session.MarketOpen ? $"Market gate until {DistrictOneSession.FormatTime(session.Schedule.MarketPassage.Closes)}"
                : harbour.Contains(now) ? "Barge unloading"
                : session.Notes.unloadingLead && now < harbour.Opens ? $"Barge at {DistrictOneSession.FormatTime(harbour.Opens)}"
                : session.Phase;
            Text(new Rect(time.x + 15, 67, 154, 20), timer, 14, Sea, true);

            if (session.ActiveScreen != Screen.World)
                return;
            Text(new Rect(20, height - 39, 410, 20), "WASD / arrows  Walk      Tab  Notebook      M  Map", 15, Cream, true,
                TextAnchor.MiddleLeft, true);

            if (!session.InDepot)
            {
                Rect mini = new(width - 196, height - 213, 176, 193);
                Card(mini, Dark, Sea);
                Text(new Rect(mini.x + 10, mini.y + 9, 156, 20),
                    session.Pin >= 0 ? DistrictOneSession.LandmarkNames[session.Pin] : "Oakhaven", 15, Cream, true);
                DrawMap(new Rect(mini.x + 14, mini.y + 32, 148, 148), false);
                if (session.Pin >= 0)
                {
                    Vector2 delta = session.LandmarkPosition(session.Pin) - session.Position;
                    string bearing = delta.magnitude < 5 ? "Here" : Bearing(delta);
                    Text(new Rect(mini.x + 9, mini.y + 171, 158, 18), $"Pin · {bearing}", 13, Gold, true);
                }
            }

            string prompt = session.Prompt;
            if (!string.IsNullOrEmpty(prompt))
            {
                Rect bar = new(width / 2 - 224, height - 86, 448, 48);
                Card(bar, Dark, Gold);
                Fill(new Rect(bar.x + 11, bar.y + 9, 33, 29), Gold);
                Text(new Rect(bar.x + 11, bar.y + 9, 33, 29), "E", 18, Ink, true, TextAnchor.MiddleCenter);
                Text(new Rect(bar.x + 56, bar.y + 2, 379, 45), prompt, 19, Cream, true, TextAnchor.MiddleLeft);
            }
        }

        private void DrawMessage(float width, float height)
        {
            bool midnight = session.ActiveScreen == Screen.Midnight;
            Color haze = midnight ? new Color32(81, 43, 116, 201) : new Color32(15, 21, 28, 159);
            Fill(new Rect(0, 0, width, height), haze);
            Rect panel = new(width / 2 - 302, height / 2 - 136, 604, 272);
            Card(panel, Cream, session.ActiveScreen == Screen.Complete ? Green : Gold);
            Text(new Rect(panel.x + 30, panel.y + 24, 544, 42), session.Title, 29, Ink, true);
            Fill(new Rect(panel.x + 31, panel.y + 71, 542, 1), Linen);
            Text(new Rect(panel.x + 30, panel.y + 93, 544, 122), session.Message, 20, Ink);
            string action = session.ActiveScreen switch
            {
                Screen.Reset => "Enter  Start another morning     Esc  Go back",
                Screen.Midnight => "E / Enter  Wake at the Depot",
                _ => session.ExtraAction == null ? "E / Enter  Continue" : $"E  Continue     Enter  {session.ExtraAction}"
            };
            Text(new Rect(panel.x + 30, panel.y + 227, 544, 22), action, 16, Green, true);
        }

        private void DrawNotebook(float width, float height)
        {
            Fill(new Rect(0, 0, width, height), new Color32(15, 21, 28, 171));
            Rect page = new(width / 2 - 420, height / 2 - 276, 840, 552);
            Card(page, Cream, Green);
            Text(new Rect(page.x + 28, page.y + 19, 630, 41), "Courier notebook", 31, Ink, true);
            Text(new Rect(page.x + 636, page.y + 25, 176, 24), $"Morning {session.Notes.mornings + 1} · District 1",
                15, Muted, true, TextAnchor.UpperRight);
            string[] tabs = { "Manifest", "Map", "Clues" };
            for (int i = 0; i < tabs.Length; i++)
            {
                Rect tab = new(page.x + 28 + i * 132, page.y + 69, 124, 35);
                bool active = (int)session.ActiveScreen - (int)Screen.Manifest == i;
                Fill(tab, active ? Green : Linen);
                Text(tab, tabs[i], 17, active ? Cream : Ink, true, TextAnchor.MiddleCenter);
            }

            switch (session.ActiveScreen)
            {
                case Screen.Manifest: DrawManifest(page); break;
                case Screen.Map: DrawBigMap(page); break;
                case Screen.Clues: DrawClues(page); break;
            }
            string help = session.ActiveScreen == Screen.Clues
                ? "← / →  Switch tab     Enter  Pin unloading lead     Tab / Esc  Close"
                : "← / →  Switch tab     ↑ / ↓  Select     Enter  Pin / unpin     Tab / Esc  Close";
            Text(new Rect(page.x + 29, page.y + 516, 783, 23), help, 15, Green, true);
        }

        private void DrawManifest(Rect page)
        {
            Text(new Rect(page.x + 29, page.y + 123, 780, 29),
                $"{session.SignedCount}/3 delivered this morning", 22, Green, true);
            for (int i = 0; i < DistrictOneSession.Recipients.Length; i++)
            {
                Rect row = new(page.x + 29, page.y + 169 + i * 89, 782, 75);
                Fill(row, session.Selection == i ? Linen : new Color32(244, 229, 200, 255));
                if (session.Selection == i)
                    Fill(new Rect(row.x, row.y, 4, row.height), Green);
                Text(new Rect(row.x + 15, row.y + 10, 355, 29), DistrictOneSession.Items[i], 21, Ink, true);
                Text(new Rect(row.x + 16, row.y + 41, 388, 23), DistrictOneSession.Recipients[i]
                    + (session.Pin == i ? "  ·  Pinned" : ""), 16, Muted);
                Text(new Rect(row.x + 415, row.y + 25, 159, 27), session.Schedule.Counter(i).Display, 17, Ink);
                Text(new Rect(row.x + 589, row.y + 25, 173, 27), session.ParcelState(i), 17, Green, true);
            }
            if (session.Notes.stampedShift)
                Text(new Rect(page.x + 29, page.y + 457, 780, 32), "Beppe’s timecard stamp earned", 17, Green, true);
        }

        private void DrawBigMap(Rect page)
        {
            Rect map = new(page.x + 35, page.y + 124, 358, 358);
            Card(map, Dark, Sea);
            DrawMap(new Rect(map.x + 12, map.y + 14, 332, 332), true);
            for (int i = 0; i < DistrictOneSession.LandmarkNames.Length; i++)
            {
                Rect row = new(page.x + 429, page.y + 130 + i * 40, 382, 35);
                if (session.Selection == i)
                {
                    Fill(row, Linen);
                    Fill(new Rect(row.x, row.y, 4, row.height), Green);
                }
                Text(new Rect(row.x + 13, row.y + 7, 355, 25),
                    $"{i + 1}.  {DistrictOneSession.LandmarkNames[i]}" + (session.Pin == i ? "  ·  Pinned" : ""),
                    17, Ink, true);
            }
            if (session.IsRouteLab)
            {
                Rect marketRow = new(page.x + 439, page.y + 414, 365, 25);
                Fill(new Rect(marketRow.x, marketRow.y + 5, 16, 4), Market);
                string market = session.MarketOpen
                    ? $"Market passage open until {DistrictOneSession.FormatTime(session.Schedule.MarketPassage.Closes)}"
                    : "Market passage closed";
                Text(new Rect(marketRow.x + 24, marketRow.y, 330, 25), market, 16, Ink, true);
            }
            if (session.Notes.arcadeCode)
                Text(new Rect(page.x + 439, page.y + 447, 365, 25),
                    $"Printers’ Arcade  ·  1842  ·  {(session.ShuttersOpen ? "Open" : "Locked")}", 16, Green, true);
        }

        private void DrawClues(Rect page)
        {
            float x = page.x + 29;
            float y = page.y + 124;
            var notes = session.Notes;
            Text(new Rect(x, y, 782, 33), "Parcel #0000", 24, Ink, true);
            string destination = notes.destination ? "Meridian House" : "________________";
            Text(new Rect(x, y + 37, 782, 26), $"Destination: {destination}     Recipient: ________     Entrance: ________",
                17, Green, true);
            Fill(new Rect(x, y + 69, 782, 1), Linen);
            float line = y + 86;
            if (!notes.fragment && !notes.printerLead && !notes.arcadeCode && !notes.unloadingLead && !notes.oldAddress)
                Text(new Rect(x, line, 782, 35), "Inspect Locker #0000 in the Depot to start a lead.", 18, Muted);
            if (notes.fragment)
                Fact(x, ref line, "Label fragment", "“… House” and a canal dispatch mark · Depot locker", false);
            if (notes.printerLead)
                Fact(x, ref line, "Printers’ passage", "Look for the dispatch plate on Town Hall’s east wall · Ivo", false);
            if (notes.arcadeCode)
                Fact(x, ref line, "Arcade code 1842", "Opens both shutters · Town Hall dispatch plate", false);
            if (notes.unloadingLead)
                Fact(x, ref line, $"Canal unloading {session.Schedule.CanalUnloading.Display}", $"{notes.unloadingSource} · Canal Bridge"
                    + (session.Pin == 6 ? " · Pinned" : ""), false);
            if (notes.oldAddress)
                Fact(x, ref line, "Witnessed address: Meridian House", "Address plate in view during unloading · Canal Bridge", true);
        }

        private void Fact(float x, ref float y, string name, string detail, bool evidence)
        {
            Text(new Rect(x, y, 782, 21), name, 17, evidence ? Green : Ink, true);
            Text(new Rect(x, y + 22, 782, 22), detail, 15, Muted);
            y += 54;
        }

        private void DrawMap(Rect rect, bool labels)
        {
            if (session.IsRouteLab)
            {
                Vector2 northBank = Project(rect, new Vector2(DistrictOneLayout.CanalBank, DistrictOneLayout.StreetMax.y));
                Vector2 southBank = Project(rect, new Vector2(DistrictOneLayout.FarBank, DistrictOneLayout.StreetMin.y));
                Fill(Rect.MinMaxRect(northBank.x, northBank.y, southBank.x, southBank.y), new Color32(37, 84, 97, 255));
                PlotStreet(rect, DistrictOneLayout.Market, Market, labels ? 2.5f : 1.4f, !session.MarketOpen);
            }
            foreach (var street in session.IsRouteLab ? DistrictOneLayout.PublicStreets : DistrictOneLayout.Original.PublicStreets)
                PlotStreet(rect, street, new Color32(156, 175, 159, 255), labels ? 2.5f : 1.4f, false);
            if (session.Notes.arcadeCode)
                PlotStreet(rect, session.IsRouteLab ? DistrictOneLayout.Arcade : DistrictOneLayout.Original.Arcade, Gold, labels ? 3 : 1.7f, !session.ShuttersOpen);
            for (int i = 0; i < DistrictOneSession.LandmarkPositions.Length; i++)
            {
                Vector2 marker = Project(rect, session.LandmarkPosition(i));
                float size = labels ? 17 : 4;
                Rect mark = new(marker.x - size / 2, marker.y - size / 2, size, size);
                Fill(mark, session.Pin == i ? Gold : Sea);
                if (labels) Text(mark, (i + 1).ToString(), 12, Ink, true, TextAnchor.MiddleCenter);
            }
            Vector2 player = Project(rect, session.InDepot ? DistrictOneLayout.Depot : session.Position);
            float playerSize = labels ? 7 : 5;
            Fill(new Rect(player.x + playerSize / 2, player.y - playerSize, playerSize, playerSize), Color.white);
            if (labels)
                Text(new Rect(rect.x + 2, rect.y + 2, 96, 20), "N ↑", 14, Cream, true);
        }

        private void PlotStreet(Rect rect, DistrictOneLayout.Street street, Color color, float thickness, bool dashed)
        {
            for (int i = 1; i < street.Points.Length; i++)
            {
                Vector2 a = Project(rect, street.Points[i - 1]);
                Vector2 b = Project(rect, street.Points[i]);
                if (!dashed)
                {
                    Line(a, b, thickness, color);
                    continue;
                }
                float length = Vector2.Distance(a, b);
                Vector2 direction = (b - a).normalized;
                for (float j = 0; j < length; j += 8)
                    Line(a + direction * j, a + direction * Mathf.Min(j + 4, length), thickness, color);
            }
        }

        private Vector2 Project(Rect rect, Vector2 point)
        {
            Vector2 min = session.IsRouteLab ? DistrictOneLayout.StreetMin : DistrictOneLayout.Original.StreetMin;
            Vector2 max = session.IsRouteLab ? DistrictOneLayout.StreetMax : DistrictOneLayout.Original.StreetMax;
            Vector2 size = max - min;
            float scale = Mathf.Min(rect.width / size.x, rect.height / size.y);
            Vector2 offset = (point - (min + max) / 2) * scale;
            return rect.center + new Vector2(offset.x, -offset.y);
        }

        private static string Bearing(Vector2 direction)
        {
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y) * 2)
                return direction.x > 0 ? "East" : "West";
            string vertical = direction.y > 0 ? "North" : "South";
            if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x) * 2)
                return vertical;
            return vertical + (direction.x > 0 ? "east" : "west");
        }

        private static void Line(Vector2 a, Vector2 b, float width, Color color)
        {
            float left = Mathf.Min(a.x, b.x) - width / 2;
            float top = Mathf.Min(a.y, b.y) - width / 2;
            Fill(new Rect(left, top, Mathf.Abs(b.x - a.x) + width, Mathf.Abs(b.y - a.y) + width), color);
        }

        private static void Card(Rect rect, Color baseColor, Color trim)
        {
            Fill(new Rect(rect.x + 4, rect.y + 5, rect.width, rect.height), new Color32(10, 19, 21, 133));
            Fill(rect, baseColor);
            Fill(new Rect(rect.x, rect.y, rect.width, 3), trim);
        }

        private void Text(Rect rect, string value, int size, Color color, bool bold = false,
            TextAnchor anchor = TextAnchor.UpperLeft, bool shadow = false)
        {
            label.fontSize = size;
            label.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            label.alignment = anchor;
            if (shadow)
            {
                label.normal.textColor = Ink;
                GUI.Label(new Rect(rect.x + 1, rect.y + 1, rect.width, rect.height), value, label);
            }
            label.normal.textColor = color;
            GUI.Label(rect, value, label);
        }

        private static void Fill(Rect rect, Color color)
        {
            Color saved = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = saved;
        }
    }
}
