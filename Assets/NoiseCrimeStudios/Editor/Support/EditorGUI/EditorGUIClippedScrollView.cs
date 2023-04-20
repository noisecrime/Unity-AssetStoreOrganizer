using UnityEngine;

namespace NoiseCrimeStudios.Editor.IMGUI
{
    /// <summary>Support for a clipped Unity IMGUI ScrollView.</summary>
    /// <remarks>
    /// When a ScrollView displays rows of items from a list Unity does not clip the rows to the view.
    /// This class provides support for clipping the rows to the view thus improving overall performance.
    /// </remarks>
    public class EditorGUIClippedScrollView
    {
        public int      ItemStartIndex          { get; set; }
        public int      ItemLastIndex           { get; set; }
        public float    PixelStartHeight        { get; set; }
        public float    PixelEndHeight          { get; set; }

        public Rect     LastScrollRectResults   { get; set; }
        public string   Information             { get; set; }

        /// <summary>Support for a clipped Unity IMGUI ScrollView.</summary>
        /// <param name="itemCount">Number of items in List being displayed in scrollview.</param>
        /// <param name="lineHeight">Universal line height of each row.</param>
        /// <param name="scrollPosY">ScrollView vertical position of scroll.</param>
        public EditorGUIClippedScrollView( int itemCount, float lineHeight, float scrollPosY )
        {
            Update( itemCount, lineHeight, scrollPosY );
        }

        public void Update( int itemCount, float lineHeight, float scrollPosY )
        {
            float   rowHeight   = lineHeight + 2f;
            int     maxRows     = Mathf.FloorToInt( LastScrollRectResults.height / rowHeight ) + 2;
            ItemStartIndex      = Mathf.FloorToInt( scrollPosY / rowHeight );
            ItemLastIndex       = ItemStartIndex + maxRows;

            PixelStartHeight    = ItemStartIndex * rowHeight;
            PixelEndHeight      = itemCount * rowHeight - ItemStartIndex * rowHeight - maxRows * rowHeight;

            if ( ItemLastIndex > itemCount )
                ItemLastIndex = itemCount;
            if ( PixelEndHeight < 0f )
                PixelEndHeight = 0f;

            Information = string.Format
                (
                "Information: Count: {0}  Rows: {1}   Index: [ {2} | {3} | {4} ]  scrollPos: {5}  Height [ Rows: {6} View: {7} total {8} / {9} ]  pixel: {10} / {11}",
                itemCount, maxRows,
                ItemStartIndex, ItemLastIndex, itemCount - ItemLastIndex,
                scrollPosY,
                maxRows * rowHeight,
                LastScrollRectResults.height,
                itemCount * rowHeight,
                PixelStartHeight + PixelEndHeight + LastScrollRectResults.height,
                PixelStartHeight, PixelEndHeight
                );
        }
    }
}
