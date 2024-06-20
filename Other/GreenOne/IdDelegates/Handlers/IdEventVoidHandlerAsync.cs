using Cysharp.Threading.Tasks;
using System;

namespace GreenOne
{
    public delegate UniTask IdEventVoidHandlerAsync(object sender, EventArgs e);
    public delegate UniTask IdEventVoidHandlerAsync<TE>(object sender, TE e);
}
