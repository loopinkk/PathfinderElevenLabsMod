using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Kingmaker.Blueprints.Encyclopedia;
using AngleSharp;
using AngleSharp.Dom;
using INode = AngleSharp.Dom.INode;

namespace SpeechMod;

public static class TextParser
{
  public static async Task<(string finalName, string text)> MakeTextForVoice(string text, string name)
  {
    if (name == Constants.Narrator)
      return (name, text);

    var (narratorValues, speakerValues) = await SeparateNarratorAndSpeaker(text);

    return string.IsNullOrEmpty(speakerValues)
      ? (Constants.Narrator, narratorValues) 
      : (name, speakerValues);
  }

  private static async Task<(string narratorValue, string speakerValue)> SeparateNarratorAndSpeaker(string markup)
    {
        var validMarkup = PreprocessMarkup(markup);

        var config = Configuration.Default;
        var context = BrowsingContext.New(config);
        var document = await context.OpenAsync(req => req.Content($"<root>{validMarkup}</root>")); // Wrap in <root> to ensure valid parsing

        List<string> narratorText = new();
        List<string> speakerText = new();

        foreach (var node in document.Body?.FirstChild?.ChildNodes ?? Enumerable.Empty<INode>())
        {
            ExtractTextFromNodes(node, isNarrator: false, narratorText, speakerText);
        }

        return (string.Join(" ", narratorText).Trim(), string.Join(" ", speakerText).Trim());
    }

    private static string PreprocessMarkup(string markup)
    {
        // Convert <color=#XXXXXX> to <color value="#XXXXXX">
        markup = Regex.Replace(markup, @"<color=#([0-9A-Fa-f]+)>", @"<color value=""#$1"">");

        // Convert <link="text"> to <link value="text">
        markup = Regex.Replace(markup, @"<link=""([^""]+)"">", @"<link value=""$1"">");

        return markup;
    }

    private static void ExtractTextFromNodes(INode node, bool isNarrator, List<string> narratorText, List<string> speakerText)
    {
        if (node is IElement element && element.TagName == "COLOR")
        {
            var colorValue = element.GetAttribute("value");
            if (colorValue == "#616060")
                isNarrator = true;
        }

        if (node.NodeType == NodeType.Text)
        {
            var text = node.TextContent.Trim();
            if (!string.IsNullOrEmpty(text))
            {
                if (isNarrator)
                    narratorText.Add(text);
                else
                    speakerText.Add(text);
            }
        }

        foreach (var child in node.ChildNodes)
        {
            ExtractTextFromNodes(child, isNarrator, narratorText, speakerText);
        }
    }
}