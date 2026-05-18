namespace MojiGotchi;

class GameOptions : Modal
{
	private Game _game;

	//constructor
	public GameOptions(Game game) : base(LM.Get("modaltitle_options"), Color.DarkGreen, Color.Green)
	{
		_game = game;
		// Add buttons
		base.AddMenuItem("English", game.SetAction(ActionType.SWITCHLANG_EN), Color.Yellow, false);
        base.AddMenuItem("Dutch", game.SetAction(ActionType.SWITCHLANG_NL), Color.Yellow, false);
		base.AddMenuItem(LM.Get("devmodeon"), game.SetAction(ActionType.DEVMODE_ON), Color.Red, false);
		base.AddMenuItem(LM.Get("devmodeoff"), game.SetAction(ActionType.DEVMODE_OFF), Color.Red, false);
	}

	public void UpdatePage(Vec2 size)
	{
		SetSpriteBg(size);
		ClearContentSprite(size);
		// Render intro text
		string text = LM.Get("modaltext_options_intro") + "\n";
		text += " \n";
		text += LM.Get("modaltext_options_langprefix") + LM.Get(LM.CurrentLang()) + "\n";
		text += " \n";
		text += LM.Get("modaltext_options_devmodeprefix") + (Program.GameOptions["devmode"] == "true" ? LM.Get("on") : LM.Get("off")) + "\n";
		string[] lines = text.Split('\n');
		for (int i = 0; i < lines.Length; i++)
		{
			string line = lines[i].TrimEnd('\r');
			if (string.IsNullOrEmpty(line)) continue;

			Sprite lineSprite = new Sprite(new Vec2(line.Length, 1));
			for (int x = 0; x < line.Length; x++)
			{
				lineSprite.WriteCell(new Vec2(x, 0), new ScreenCell
				{
					Character = line[x],
					Color = Color.White,
					BgColor = base.BgColor
				});
			}

			AddContent(lineSprite, new Vec2(4, i + 2));
		}
		UpdateMenu();
	}

	private void UpdateMenu()
	{
		foreach(MenuItem item in base.Options)
		{
			//english
			if (item.Action.MyActionType == ActionType.SWITCHLANG_EN)
			{
				if (LM.CurrentLang() == "en")
				{
					item.Disable();
				}
				else
				{
					item.Enable();
				}
			}
			//dutch
			if (item.Action.MyActionType == ActionType.SWITCHLANG_NL)
			{
				if (LM.CurrentLang() == "nl")
				{
					item.Disable();
				}
				else
				{
					item.Enable();
				}
			}
			if (item.Action.MyActionType == ActionType.DEVMODE_ON)
			{
				if (Program.GameOptions["devmode"] == "true")
				{
					item.Disable();
				}
				else
				{
					item.Enable();
				}
			}
			if (item.Action.MyActionType == ActionType.DEVMODE_OFF)
			{
				if (Program.GameOptions["devmode"] == "false")
				{
					item.Disable();
				}
				else
				{
					item.Enable();
				}
			}
		}
	}
}