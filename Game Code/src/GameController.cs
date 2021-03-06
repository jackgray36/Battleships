﻿using System;
using System.Linq;
using System.Collections.Generic;
using SwinGameSDK;

namespace MyGame
{
    /// <summary>
    /// The GameController is responsible for controlling the game,
    /// managing user input, and displaying the current state of the
    /// game.
    /// </summary>
    public static class GameController
    {
        private static BattleShipsGame _theGame;
        private static Player _human;
        private static AIPlayer _ai;

        private static Stack<GameState> _state = new Stack<GameState>();

        private static AIOption _aiSetting = AIOption.Easy;

        /// <summary>
        /// Returns the current state of the game, indicating which screen is
        /// currently being used.
        /// <value>The current state</value>
        /// <returns>The current state</returns>
        /// </summary>
        public static GameState CurrentState
        {
            get
            {
                return _state.Peek();
            }
        }

        /// <summary>
        /// Returns the human player.
        /// <value>the human player</value>
        /// <returns>the human player</returns>
        /// </summary>
        public static Player HumanPlayer
        {
            get
            {
                return _human;
            }
        }

        /// <summary>
        /// Returns the computer player.
        /// <value>the computer player</value>
        /// <returns>the conputer player</returns>
        /// </summary>
        public static Player ComputerPlayer
        {
            get
            {
                return _ai;
            }
        }

        static GameController()
        {
            // Bottom state will be quitting. If player exits main menu then the game is over.
            _state.Push(GameState.Quitting);

            // At the start the player is viewing the main menu.
            _state.Push(GameState.ViewingMainMenu);
        }

        /// <summary>
        /// Starts a new game.
        /// <remarks>
        /// Creates an AI player based upon the _aiSetting.
        /// </remarks>
        /// </summary>
        public static void StartGame()
        {
            if (_theGame != null)
                EndGame();

            // Create the game.
            _theGame = new BattleShipsGame();

            // Create the players.
            switch (_aiSetting)
            {
                case AIOption.Easy:
                    {
                        _ai = new AIEasyPlayer(_theGame);
                        break;
                    }
                case AIOption.Medium:
                    {
                        _ai = new AIMediumPlayer(_theGame);
                        break;
                    }

                case AIOption.Hard:
                    {
                        _ai = new AIHardPlayer(_theGame);
                        break;
                    }
                default:
                    {
                        throw new ArgumentException("_aiSetting did not hold an expected value");
                    }
                
            }

            _human = new Player(_theGame);

            // AddHandler _human.PlayerGrid.Changed, AddressOf GridChanged
            _ai.PlayerGrid.Changed += GridChanged;
            _theGame.AttackCompleted += AttackCompleted;

            AddNewState(GameState.Deploying);
        }

        /// <summary>
        /// Stops listening to the old game once a new game is started
        /// </summary>
        private static void EndGame()
        {
            // RemoveHandler _human.PlayerGrid.Changed, AddressOf GridChanged
            _ai.PlayerGrid.Changed -= GridChanged;
            _theGame.AttackCompleted -= AttackCompleted;
        }

        /// <summary>
        /// Listens to the game grids for any changes and redraws the screen
        /// when the grids change.
        /// <param name="sender">the grid that changed</param>
        /// <param name="args">not used</param>
        /// </summary>
        private static void GridChanged(object sender, EventArgs args)
        {
            DrawScreen();
            SwinGame.RefreshScreen(60);
        }

        private static void PlayHitSequence(int row, int column, bool showAnimation)
        {
            if (showAnimation)
                UtilityFunctions.AddExplosion(row, column);

            Audio.PlaySoundEffect(GameResources.GameSound("Hit"), UtilityFunctions.VolumeLevel);

            UtilityFunctions.DrawAnimationSequence();
        }

        private static void PlayMissSequence(int row, int column, bool showAnimation)
        {
            if (showAnimation)
                UtilityFunctions.AddSplash(row, column);

            Audio.PlaySoundEffect(GameResources.GameSound("Miss"), UtilityFunctions.VolumeLevel);

            UtilityFunctions.DrawAnimationSequence();
        }

        /// <summary>
        /// Listens for attacks to be completed.
        /// <param name="sender">the game</param>
        /// <param name="result">the result of the attack</param>
        /// <remarks>
        /// Displays a message, plays sound and redraws the screen
        /// </remarks>
        /// </summary>
        private static void AttackCompleted(object sender, AttackResult result)
        {
            bool isHuman;
            isHuman = _theGame.Player == HumanPlayer;

            if (isHuman)
                UtilityFunctions.Message = "You " + result.ToString();
            else
                UtilityFunctions.Message = "The AI " + result.ToString();

            switch (result.Value)
            {
                case ResultOfAttack.Destroyed:
                    {
                        PlayHitSequence(result.Row, result.Column, isHuman);
                        Audio.PlaySoundEffect(GameResources.GameSound("Sink"), UtilityFunctions.VolumeLevel);
                        break;
                    }

                case ResultOfAttack.GameOver:
                    {
                        PlayHitSequence(result.Row, result.Column, isHuman);
                        Audio.PlaySoundEffect(GameResources.GameSound("Sink"), UtilityFunctions.VolumeLevel);

                        while (Audio.SoundEffectPlaying(GameResources.GameSound("Sink")))
                        {
                            SwinGame.Delay(10);
                            SwinGame.RefreshScreen(60);
                        }

                        if (HumanPlayer.IsDestroyed)
                            Audio.PlaySoundEffect(GameResources.GameSound("Lose"), UtilityFunctions.VolumeLevel);
                        else
                            Audio.PlaySoundEffect(GameResources.GameSound("Winner"), UtilityFunctions.VolumeLevel);
                        break;
                    }

                case ResultOfAttack.Hit:
                    {
                        PlayHitSequence(result.Row, result.Column, isHuman);
                        break;
                    }

                case ResultOfAttack.Miss:
                    {
                        PlayMissSequence(result.Row, result.Column, isHuman);
                        break;
                    }

                case ResultOfAttack.ShotAlready:
                    {
                        Audio.PlaySoundEffect(GameResources.GameSound("Error"), UtilityFunctions.VolumeLevel);
                        break;
                    }
            }
        }

        /// <summary>
        /// Completes the deployment phase of the game and
        /// switches to the battle mode (Discovering state).
        /// <remarks>
        /// This adds the players to the game before switching state.
        /// </remarks>
        /// </summary>
        public static void EndDeployment()
        {
            // Deploy the players.
            _theGame.AddDeployedPlayer(_human);
            _theGame.AddDeployedPlayer(_ai);

            SwitchState(GameState.Discovering);
        }

        /// <summary>
        /// Gets the player to attack the indicated row and column.
        /// <param name="row">the row to attack</param>
        /// <param name="col">the column to attack</param>
        /// <remarks>
        /// Checks the attack result once the attack is complete
        /// </remarks>
        /// </summary>
        public static void Attack(int row, int col)
        {
            AttackResult result;
            result = _theGame.Shoot(row, col);
            CheckAttackResult(result);
        }

        /// <summary>
        /// Gets the AI to attack.
        /// <remarks>
        /// Checks the attack result once the attack is complete.
        /// </remarks>
        /// </summary>
        private static void AIAttack()
        {
            AttackResult result;
            result = _theGame.Player.Attack();
            CheckAttackResult(result);
        }

        /// <summary>
        /// Checks the results of the attack and switches to
        /// ending the game if the result was game over.
        /// <param name="result">the result of the last attack</param>
        /// <remarks>
        /// Gets the AI to attack if the result switched
        /// to the AI player.
        /// </remarks>
        /// </summary>
        private static void CheckAttackResult(AttackResult result)
        {
            switch (result.Value)
            {
                case ResultOfAttack.Miss:
                    {
                        if (_theGame.Player == ComputerPlayer)
                            AIAttack();
                        break;
                    }

                case ResultOfAttack.GameOver:
                    {
                        SwitchState(GameState.EndingGame);
                        break;
                    }
            }
        }

        /// <summary>
        /// Handles the user's input.
        /// <remarks>
        /// Reads key and mouse input and converts these into
        /// actions for the game to perform. The actions
        /// performed depend upon the state of the game.
        /// </remarks>
        /// </summary>
        public static void HandleUserInput()
        {
            // Read incoming input events.
            SwinGame.ProcessEvents();

            if(SwinGame.KeyTyped(KeyCode.EqualsKey) || SwinGame.KeyTyped(KeyCode.PlusKey))
            {
                UtilityFunctions.VolumeLevel += 0.1f;
                Audio.SetMusicVolume(UtilityFunctions.VolumeLevel);
            }
            else if(SwinGame.KeyTyped(KeyCode.UnderscoreKey) || SwinGame.KeyTyped(KeyCode.MinusKey))
            {
                UtilityFunctions.VolumeLevel -= 0.1f;
                Audio.SetMusicVolume(UtilityFunctions.VolumeLevel);
            }

            // Switch through themes
            if (SwinGame.KeyTyped(KeyCode.TKey))
            {
                GameResources.GameTheme++;
                if(!Enum.IsDefined(typeof(Theme), GameResources.GameTheme))
                {
                    GameResources.GameTheme = 0;
                }
            }

            switch (CurrentState)
            {
                case GameState.ViewingMainMenu:
                    {
                        MenuController.HandleMainMenuInput();
                        break;
                    }

                case GameState.ViewingGameMenu:
                    {
                        MenuController.HandleGameMenuInput();
                        break;
                    }

                case GameState.AlteringSettings:
                    {
                        MenuController.HandleSetupMenuInput();
                        break;
                    }
                case GameState.Rules:
                    {
                        HighScoreController.HandleHighScoreInput();
                        break;
                    }

                case GameState.Controls:
                    {
                        MenuController.HandleControlersMenu();
                        break;
                    }

                case GameState.AlteringVolume:
                    {
                        if (_state.Skip(1).First() == GameState.ViewingGameMenu)
                        {
                            MenuController.HandleVolumeMenuInput(1);
                        }
                        else if (_state.Skip(1).First() == GameState.ViewingMainMenu)
                        {
                            MenuController.HandleVolumeMenuInput(0);
                        }
                        break;
                    }

                case GameState.Deploying:
                    {
                        DeploymentController.HandleDeploymentInput();
                        break;
                    }

                case GameState.Discovering:
                    {
                        DiscoveryController.HandleDiscoveryInput();
                        break;
                    }

                case GameState.EndingGame:
                    {
                        EndingGameController.HandleEndOfGameInput();
                        break;
                    }

                case GameState.ViewingHighScores:
                    {
                        HighScoreController.HandleHighScoreInput();
                        break;
                    }
            }

            UtilityFunctions.UpdateAnimations();
        }

        /// <summary>
        /// Draws the current state of the game to the screen.
        /// <remarks>
        /// What is drawn depends upon the state of the game.
        /// </remarks>
        /// </summary>
        public static void DrawScreen()
        {
            UtilityFunctions.DrawBackground();

            switch (CurrentState)
            {
                case GameState.ViewingMainMenu:
                    {
                        MenuController.DrawMainMenu();
                        break;
                    }

                case GameState.ViewingGameMenu:
                    {
                        MenuController.DrawGameMenu();
                        break;
                    }

                case GameState.AlteringSettings:
                    {
                        MenuController.DrawSettings();
                        break;
                    }
                case GameState.Rules:
                    {
                        MenuController.drawRules();
                        break;
                    }
                case GameState.Controls:
                    {
                        MenuController.drawControls();
                        break;
                    }

                case GameState.AlteringVolume:
                    {
                        if(_state.Skip(1).First() == GameState.ViewingGameMenu)
                        {
                            MenuController.DrawVolumeSettings(1);
                        }
                        else if(_state.Skip(1).First() == GameState.ViewingMainMenu)
                        {
                            MenuController.DrawVolumeSettings(0);
                        }
                        break;
                    }

                case GameState.Deploying:
                    {
                        DeploymentController.DrawDeployment();
                        break;
                    }

                case GameState.Discovering:
                    {
                        DiscoveryController.DrawDiscovery();
                        break;
                    }

                case GameState.EndingGame:
                    {
                        EndingGameController.DrawEndOfGame();
                        break;
                    }

                case GameState.ViewingHighScores:
                    {
                        HighScoreController.DrawHighScores();
                        break;
                    }
            }

            UtilityFunctions.DrawAnimations();

            SwinGame.RefreshScreen(60);
        }

        /// <summary>
        /// Move the game to a new state. The current state is maintained
        /// so that it can be returned to.
        /// <param name="state">the new game state</param>
        /// </summary>
        public static void AddNewState(GameState state)
        {
            _state.Push(state);
            UtilityFunctions.Message = "";
        }

        /// <summary>
        /// End the current state and add in the new state.
        /// <param name="newState">the new state of the game</param>
        /// </summary>
        public static void SwitchState(GameState newState)
        {
            EndCurrentState();
            AddNewState(newState);
        }

        /// <summary>
        /// Ends the current state, returning to the prior state.
        /// </summary>
        public static void EndCurrentState()
        {
            _state.Pop();
        }

        /// <summary>
        /// Sets the difficulty for the next match.
        /// <param name="setting">the new difficulty level</param>
        /// </summary>
        public static void SetDifficulty(AIOption setting)
        {
            _aiSetting = setting;
        }
    }
}