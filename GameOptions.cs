namespace MojiGotchi;

class GameOptions : Modal
{
	private string _optionsIntroText = LM.Get("editor_config_introtext");
	private Game _game;

	//constructor
	public GameOptions(Game game) : base(LM.Get("modaltitle_options"), Color.DarkYellow, Color.Yellow)
	{
		_game = game;
		// Add buttons
		base.AddMenuItem("English", game.SetAction(ActionType.SWITCHLANG_EN), Color.Red, false);
        base.AddMenuItem("Dutch", game.SetAction(ActionType.SWITCHLANG_NL), Color.Red, false);
	}

	public void UpdatePage(Vec2 size)
	{
		SetSpriteBg(size);
		ClearContentSprite(size);
		// Render intro text
		string text = _optionsIntroText;
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
	}
}