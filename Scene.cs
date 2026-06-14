using System;
using System.Collections.Generic;

public class Scene
{
    public string Id { get; set; }
    public string Description { get; set; }
    public List<Choice> Choices { get; set; } = new();
    public string? Art { get; set; }

    // NEW: Runs when the player enters the scene
    public Action<Player>? OnEnter { get; set; }

    // NEW: Allows logic-based branching (puzzles, checks, etc.)
    public Func<Player, string>? NextSceneLogic { get; set; }
}
