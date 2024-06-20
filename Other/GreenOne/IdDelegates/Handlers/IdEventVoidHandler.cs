using System;

namespace GreenOne
{
    /// <summary>
    /// Класс, представляющий один из возможных делегатов для <see cref="IdDelegate{D}"/>.
    /// </summary>
    public delegate void IdEventVoidHandler(object sender, EventArgs e);
    /// <summary>
    /// Класс, представляющий один из возможных делегатов для <see cref="IdDelegate{D}"/>.
    /// </summary>
    public delegate void IdEventVoidHandler<TE>(object sender, TE e);
}
