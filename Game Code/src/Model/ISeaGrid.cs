﻿using System;

namespace MyGame
{
    /// <summary>
    /// The ISeaGrid defines the read only interface of a Grid. This
    /// allows each player to see and attack their opponents grid.
    /// </summary>
    public interface ISeaGrid
    {
        int Width { get; }

        int Height { get; }

        /// <summary>
        /// Indicates that the grid has changed.
        /// </summary>
        event EventHandler Changed;

        /// <summary>
        /// Provides access to the given row/column
        /// <param name="row">the row to access</param>
        /// <param name="column">the column to access</param>
        /// <value>what the player can see at that location</value>
        /// <returns>what the player can see at that location</returns>
        /// </summary>
        TileView Item(int x, int y);

        /// <summary>
        /// Mark the indicated tile as shot
        /// <param name="row">the row of the tile</param>
        /// <param name="col">the column of the tile</param>
        /// <returns>the result of the attack</returns>
        /// </summary>
        AttackResult HitTile(int row, int col);
    }
}