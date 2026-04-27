namespace MojiGotchi;

class EditorConfig : Modal
{
	private string _configIntroText = LM.Get("editor_config_introtext");
	private Editor _editor;
	private Level _level;

	//constructor
	public EditorConfig(Editor editor) : base("Editor Config", Color.DarkYellow, Color.Yellow)
	{
		_editor = editor;
		_level = _editor.GetLevel();
		// Add buttons
		base.AddMenuItem(LM.Get("editor_config_h_plus"), _editor.SetEditorAction(ActionType.EDITOR_SIZEHPLUS), Color.WoodDark);
		base.AddMenuItem(LM.Get("editor_config_h_min"), _editor.SetEditorAction(ActionType.EDITOR_SIZEHMIN), Color.WoodDark);
		base.AddMenuItem(LM.Get("editor_config_v_plus"), _editor.SetEditorAction(ActionType.EDITOR_SIZEVPLUS), Color.WoodDark);
		base.AddMenuItem(LM.Get("editor_config_v_min"), _editor.SetEditorAction(ActionType.EDITOR_SIZEVMIN), Color.WoodDark);
	}

	public void UpdatePage(Vec2 size)
	{
		SetSpriteBg(size);
		ClearContentSprite(size);

		_level = _editor.GetLevel();
		//construct level info string from level object obtained from editor
		string levelInfoText = $"{LM.Get("editor_config_levelsize")}[{_level.Size.X} X {_level.Size.Y}]\n";

		// Render intro text
		string text = _configIntroText + levelInfoText;
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