public static class WizardAdventure
{
    public static Dictionary<string, Scene> Build()
    {
        return new Dictionary<string, Scene>
        {
            {
                "intro",
                new Scene
                {
                    Art = @"
        /\    
       /  \   _ __   ___  _ __  
      / /\ \ | '_ \ / _ \| '_ \ 
     / ____ \| | | | (_) | | | |
    /_/    \_\_| |_|\___/|_| |_|",
                    Description = "You awaken in a dimly lit chamber. Candles flicker. A stone door stands before you.",
                    Choices =
                    {
                        new Choice { Text = "Approach the stone door", NextSceneId = "door" },
                        new Choice { Text = "Examine the room", NextSceneId = "room_search" }
                    }
                }
            },

            {
                "room_search",
                new Scene
                {
                    Description = "You search the dusty chamber. Beneath a loose stone, you find a glowing **Rune Key**.",
                    Choices =
                    {
                        new Choice { Text = "Take the Rune Key", NextSceneId = "take_key" },
                        new Choice { Text = "Leave it", NextSceneId = "intro" }
                    }
                }
            },

            {
                "take_key",
                new Scene
                {
                    Description = "You take the Rune Key. It hums with ancient power.",
                    OnEnter = player => player.AddItem("Rune Key"),
                    Choices =
                    {
                        new Choice { Text = "Return to the stone door", NextSceneId = "door" }
                    }
                }
            },

            {
                "door",
                new Scene
                {
                    Art = @"
        __________
       |  __  __  |
       | |  ||  | |
       | |  ||  | |
       | |__||__| |
       |  __  __()|
       | |  ||  | |
       | |  ||  | |
       | |__||__| |
       |__________|",
                    Description = "A massive stone door blocks your path. Runes glow faintly.",
                    Choices =
                    {
                        new Choice { Text = "Touch the runes", NextSceneId = "door_touch" },
                        new Choice { Text = "Step back", NextSceneId = "intro" }
                    }
                }
            },

            {
                "door_touch",
                new Scene
                {
                    Description = "The runes flare brightly. A voice whispers: 'Only the bearer of the Rune Key may pass.'",
                    Choices =
                    {
                        new Choice { Text = "Attempt to open the door", NextSceneId = "door_attempt" },
                        new Choice { Text = "Retreat", NextSceneId = "intro" }
                    }
                }
            },

            {
                "door_attempt",
                new Scene
                {
                    Description = "You place your hand upon the runes...",
                    NextSceneLogic = (player) =>
                    {
                        if (player.HasItem("Rune Key"))
                            return "door_open";
                        else
                            return "door_fail";
                    }
                }
            },

            {
                "door_fail",
                new Scene
                {
                    Description = "A blast of arcane energy erupts! You are thrown back.",
                    OnEnter = player => player.TakeDamage(5),
                    Choices =
                    {
                        new Choice { Text = "Stagger to your feet", NextSceneId = "intro" }
                    }
                }
            },

            {
                "door_open",
                new Scene
                {
                    Art = @"
      __________
     / ________ \
    / /  __  __\ \
   / /  |  ||  | \ \
  / /   |  ||  |  \ \
 ( (    |__||__|   ) )
  \ \            / /
   \ \__________/ /
    \____________/",
                    Description = "The door rumbles open. A warm light spills out. You step into a grand hall.",
                    Choices =
                    {
                        new Choice { Text = "Explore the hall", NextSceneId = "hall" }
                    }
                }
            },

            {
                "hall",
                new Scene
                {
                    Description = "The hall stretches endlessly. A fountain of shimmering water stands in the center.",
                    Choices =
                    {
                        new Choice { Text = "Drink from the fountain", NextSceneId = "fountain" },
                        new Choice { Text = "Continue forward", NextSceneId = "boss_entrance" }
                    }
                }
            },

            {
                "fountain",
                new Scene
                {
                    Description = "You drink the enchanted water. Your wounds knit together.",
                    OnEnter = player => player.Heal(10),
                    Choices =
                    {
                        new Choice { Text = "Return to the hall", NextSceneId = "hall" }
                    }
                }
            },

            {
                "boss_entrance",
                new Scene
                {
                    Art = @"
        (    )
         (oo)
  /-------\/
 / |     ||
*  ||----||
   ^^    ^^",
                    Description = "A shadow looms. A Guardian Beast blocks your path.",
                    Choices =
                    {
                        new Choice { Text = "Fight the beast", NextSceneId = "boss_fight" },
                        new Choice { Text = "Run away", NextSceneId = "hall" }
                    }
                }
            },

            {
                "boss_fight",
                new Scene
                {
                    Description = "The beast charges!",
                    OnEnter = player => player.TakeDamage(15),
                    NextSceneLogic = (player) =>
                    {
                        if (player.Health <= 0)
                            return "death";
                        else
                            return "victory";
                    }
                }
            },

            {
                "death",
                new Scene
                {
                    Art = @"
       _____
      /     \
     | () () |
      \  ^  /
       |||||
       |||||",
                    Description = "Your vision fades... The adventure ends here.",
                    Choices = { }
                }
            },

            {
                "victory",
                new Scene
                {
                    Art = @"
 __     __          __          ___       
 \ \   / /          \ \        / (_)      
  \ \_/ /__  _   _   \ \  /\  / / _ _ __  
   \   / _ \| | | |   \ \/  \/ / | | '_ \ 
    | | (_) | |_| |    \  /\  /  | | | | |
    |_|\___/ \__,_|     \/  \/   |_|_| |_|",
                    Description = "The beast collapses. You have triumphed. The path ahead is yours to claim.",
                    Choices = { }
                }
            }
        };
    }
}
