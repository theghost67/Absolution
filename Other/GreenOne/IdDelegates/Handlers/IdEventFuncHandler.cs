using System;

namespace GreenOne
{
    /// <summary>
    /// Класс, представляющий один из возможных делегатов для <see cref="IdDelegate{D}"/>.
    /// </summary>
    public delegate TR IdEventFuncHandler<TR>(object sender, EventArgs e);
    /// <summary>
    /// Класс, представляющий один из возможных делегатов для <see cref="IdDelegate{D}"/>.
    /// </summary>
    public delegate TR IdEventFuncHandler<TR, TE>(object sender, TE e);
}
