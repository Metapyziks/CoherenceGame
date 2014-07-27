using UnityEngine;

using System.Collections;
using System.Linq;

public static class GUITextExtensions
{
    public static void SetTextWithWrapping(this GUIText gui, string text, float width)
    {
        var words = text.Split(' ');

        gui.text = words[0];

        foreach (var word in words.Skip(1)) {
            var prev = gui.text;

            gui.text += " " + word;

            var newWidth = gui.GetScreenRect().width;

            if (newWidth > width) {
                gui.text = prev + "\n" + word;
            }
        }
    }
}
