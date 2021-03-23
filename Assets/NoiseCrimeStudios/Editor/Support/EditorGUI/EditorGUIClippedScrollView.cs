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
        public int      itemStartIndex          { get; set; }
        public int      itemLastIndex           { get; set; }
        public float    pixelStartHeight        { get; set; }
        public float    pixelEndHeight          { get; set; }

        public Rect     lastScrollRectResults   { get; set; }
        public string   information             { get; set; }

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
            int     maxRows     = Mathf.FloorToInt( lastScrollRectResults.height / rowHeight ) + 2;
            itemStartIndex      = Mathf.FloorToInt( scrollPosY / rowHeight );
            itemLastIndex       = itemStartIndex + maxRows;

            pixelStartHeight    = itemStartIndex * rowHeight;
            pixelEndHeight      = itemCount * rowHeight - itemStartIndex * rowHeight - maxRows * rowHeight;

            if ( itemLastIndex > itemCount ) itemLastIndex = itemCount;
            if ( pixelEndHeight < 0f ) pixelEndHeight = 0f;

            information = string.Format
                (
                "Information: Count: {0}  Rows: {1}   Index: [ {2} | {3} | {4} ]  scrollPos: {5}  Height [ Rows: {6} View: {7} total {8} / {9} ]  pixel: {10} / {11}",
                itemCount, maxRows,
                itemStartIndex, itemLastIndex, itemCount - itemLastIndex,
                scrollPosY,
                maxRows * rowHeight,
                lastScrollRectResults.height,
                itemCount * rowHeight,
                pixelStartHeight + pixelEndHeight + lastScrollRectResults.height,
                pixelStartHeight, pixelEndHeight
                );
        }
    }
}
