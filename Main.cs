using Godot;

public partial class Main : Node2D
{
	private CharacterBody2D player;

	public override void _Ready()
	{
		// 获取 Player 节点
		player = GetNode<CharacterBody2D>("Player");
		
		if (player != null)
		{
			// 确保玩家在等距地图中央 (960, 450)
			player.Position = new Vector2(960, 450);
			GD.Print($"Player positioned at: {player.Position}");
			
			// 新的 Player 场景已经自带启用的 Camera2D，无需额外设置
			GD.Print("Player loaded with built-in camera system");
		}
		else
		{
			GD.PrintErr("Player node not found");
		}
	}
}
