namespace GreenOne
{
    /// <summary>
    /// Перечисление, содержащее стороны привязки дочерних элементов к родителю.
    /// </summary>
    public enum AlignAnchor
    {
        Left = 1,
        Center = 2,
        Right = 4,

        Top = 8,
        Middle = 16,
        Bottom = 32,

        TopLeft = Top | Left,
        TopCenter = Top | Center,
        TopRight = Top | Right,

        MiddleLeft = Middle | Left,
        MiddleCenter = Middle | Center,
        MiddleRight = Middle | Right,

        BottomLeft = Bottom | Left,
        BottomCenter = Bottom | Center,
        BottomRight = Bottom | Right,
    }
}
