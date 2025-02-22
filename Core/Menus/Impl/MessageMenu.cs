using Helion.Audio.Sounds;
using Helion.Menus.Base;
using Helion.Menus.Base.Text;
using Helion.Resources.Archives.Collection;
using Helion.Util.Configs;
using Helion.Util.Consoles;
using System;
using System.Collections.Generic;
using Helion.Window;
using Helion.Window.Input;

namespace Helion.Menus.Impl;

public class MessageMenu : Menu
{
    public readonly bool IsYesNoConfirm;
    public readonly bool ClearMenus;

    // True if IsYesNoConfirm and Y was pressed
    public event EventHandler<bool>? Cleared;

    public MessageMenu(IConfig config, HelionConsole console, SoundManager soundManager, ArchiveCollection archiveCollection,
        IList<string> text, bool isYesNoConfirm = false, bool clearMenus = true)
        : base(config, console, soundManager, archiveCollection, 90, clearOnClose: clearMenus)
    {
        IsYesNoConfirm = isYesNoConfirm;
        ClearMenus = clearMenus;

        for (int i = 0; i < text.Count; i++)
        {
            Components = Components.Add(new MenuSmallTextComponent(text[i]));
            if (i != text.Count - 1)
                Components = Components.Add(new MenuPaddingComponent(8));
        }

        SetToFirstActiveComponent();
    }

    public bool ShouldClear(IConsumableInput input)
    {
        if (IsYesNoConfirm)
        {
            if (input.ConsumeKeyPressed(Key.Y) || input.ConsumeKeyPressed(Key.ButtonA))
            {
                Cleared?.Invoke(this, true);
                return true;
            }
            if (input.ConsumeKeyPressed(Key.N) || input.ConsumeKeyPressed(Key.ButtonB))
            {
                Cleared?.Invoke(this, false);
                return true;
            }
        }
        else if (input.Manager.HasAnyKeyPressed())
        {
            Cleared?.Invoke(this, false);
            return true;
        }

        return false;
    }
}
