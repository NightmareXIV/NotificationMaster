using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.ImGuiNotification;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NotificationMaster
{
    static class Static
    {
        public static void AddShifting<T>(this T[] array, T element)
        {
            for(var i = array.Length - 2; i >= 0; i--)
            {
                array[i + 1] = array[i];
            }
            array[0] = element;
        }

        public static string NotNull(this string s)
        {
            return s == null ? "" : s;
        }

        public static string ReplaceAll(this string s, string[][] replacementArray, ReplaceType replaceType)
        {
            if(replaceType == ReplaceType.JSON)
            {
                foreach (var e in replacementArray)
                {
                    s = s.Replace(e[0], e[1].cleanForJSON());
                }
            }
            else if (replaceType == ReplaceType.URLEncode)
            {
                foreach (var e in replacementArray)
                {
                    s = s.Replace(e[0], HttpUtility.UrlEncode(e[1]));
                }
            }
            else
            {
                foreach (var e in replacementArray)
                {
                    s = s.Replace(e[0], e[1]);
                }
            }
            return s;
        }

        public enum ReplaceType { Normal, JSON, URLEncode }

        public static string cleanForJSON(this string s)
        {
            if (s == null || s.Length == 0)
            {
                return "";
            }

            char c = '\0';
            int i;
            int len = s.Length;
            StringBuilder sb = new StringBuilder(len + 4);
            String t;

            for (i = 0; i < len; i += 1)
            {
                c = s[i];
                switch (c)
                {
                    case '\\':
                    case '"':
                        sb.Append('\\');
                        sb.Append(c);
                        break;
                    case '/':
                        sb.Append('\\');
                        sb.Append(c);
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    default:
                        if (c < ' ')
                        {
                            t = "000" + String.Format("X", c);
                            sb.Append("\\u" + t.Substring(t.Length - 4));
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
            return sb.ToString();
        }

        public static void ImGuiToggleIconButton(ref bool setting, FontAwesomeIcon icon, string tooltip = null)
        {
            var colored = setting;
            if (colored)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, 0x6600b500);
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0x6600b500);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0x6600b500);
            }
            if(ImGuiIconButton(icon, tooltip))
            {
                setting = !setting;
            }
            if (colored) ImGui.PopStyleColor(3);
        }

        public static bool ImGuiIconButton(FontAwesomeIcon icon, string tooltip)
        {
            ImGui.PushFont(UiBuilder.IconFont);
            var result = ImGui.Button($"{icon.ToIconString()}##{icon.ToIconString()}-{tooltip}");
            ImGui.PopFont();


            if (tooltip != null)
                ImGuiTextTooltip(tooltip);


            return result;
        }

        public static void ImGuiTextTooltip(string text)
        {
            if (ImGui.IsItemHovered())
            {
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
                ImGui.BeginTooltip();
                ImGui.TextUnformatted(text);
                ImGui.EndTooltip();
                ImGui.PopStyleVar();
            }
        }
    }
}
