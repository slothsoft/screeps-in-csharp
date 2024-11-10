using System;
using System.Collections.Generic;
using FriendlyWorldBot.Rooms.Structures;
using FriendlyWorldBot.Utils;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Gui;

public static class RoomVisualExtensions {

    public static IRoomVisual HorizontalLine(this IRoomVisual visual, FractionalPosition startPos, double width, LineVisualStyle? style = null) {
        return visual.Line(startPos, new FractionalPosition(startPos.X + width, startPos.Y), style);
    }
    
    public static IRoomVisual StorageTextBlock(this IRoomVisual visual, IStructure start, double x, double y, double width, double step,
        out double currentY, TextVisualStyle? leftStyle = null, TextVisualStyle? rightStyle = null) {
        return visual.DictionaryTextBlock(start, x, y, width, step, mo => mo.ContainedResourceTypes(), (m, k) => null,
            (m, k) => ((IWithStore)m).Store[k].ToString(), out currentY, leftStyle, rightStyle);
    }

    public static IRoomVisual MemoryTextBlock(this IRoomVisual visual, IMemoryObject start, double x, double y, double width, double step,
                out double currentY, TextVisualStyle? leftStyle = null, TextVisualStyle? rightStyle = null) {
        return visual.DictionaryTextBlock(start, x, y, width, step, mo => mo.Keys, (m, k) => m.TryGetObject(k, out var o) ? o : null,
            (m, k) => m.TryGetAny(k).ToString() ?? string.Empty, out currentY, leftStyle, rightStyle);
    }

    public static IRoomVisual DictionaryTextBlock<TNode, TKey>(this IRoomVisual visual, TNode start, double x, double y, double width, double step,
                Func<TNode, IEnumerable<TKey>> getKeys, Func<TNode, TKey, TNode?> getChildNode, Func<TNode, TKey, string> getValue,
                out double currentY, TextVisualStyle? leftStyle = null, TextVisualStyle? rightStyle = null) {
        currentY = y;
        foreach (var key in getKeys(start)) {
            var nullsafeKey = key?.ToString() ?? string.Empty;
            var childNode = getChildNode(start, key);
            if (childNode != null) {
                visual.Text(nullsafeKey, new FractionalPosition(x, currentY), leftStyle);
                visual.DictionaryTextBlock(childNode, x + step / 2 , currentY + step, width - step / 2, step, getKeys, getChildNode, getValue, out var newY, leftStyle, rightStyle);
                currentY = newY;
            } else {
                visual.KeyValueText(nullsafeKey, getValue(start, key), x, currentY, width, leftStyle, rightStyle);
                currentY += step;
            }
        }
        return visual;
    }

    public static IRoomVisual KeyValueText(this IRoomVisual visual, string key, string value, double x, double y, double width,
                TextVisualStyle? leftStyle = null, TextVisualStyle? rightStyle = null) {
        visual.Text(key, new FractionalPosition(x, y), leftStyle);
        visual.Text(value, new FractionalPosition(x + width, y), rightStyle);
        return visual;
    }
}