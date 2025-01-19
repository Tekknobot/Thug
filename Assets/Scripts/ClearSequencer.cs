using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearSequencer : MonoBehaviour
{
    public ScrollableGrid grid; // Reference to the ScrollableGrid

    /// <summary>
    /// Clears the sequencer by turning all cells off.
    /// </summary>
    [ContextMenu("Clear All Cells")] // This adds a context menu option to call the method in the editor.
    public void ClearAllCells()
    {
        if (grid == null)
        {
            Debug.LogError("Grid is not assigned!");
            return;
        }

        // Loop through all rows and columns of the grid
        for (int row = 0; row < grid.rows; row++)
        {
            for (int col = 0; col < grid.columns; col++)
            {
                CustomToggle toggle = grid.GetToggleAt(row, col);
                if (toggle != null)
                {
                    toggle.SetState(false); // Turn the cell off
                }
            }
        }

        Debug.Log("Sequencer cleared!");
    }
}
