using Cysharp.Threading.Tasks;
using System;

namespace GreenOne
{
    public delegate UniTask<TR> IdEventFuncHandlerAsync<TR>(object sender, EventArgs e);
    public delegate UniTask<TR> IdEventFuncHandlerAsync<TR, TE>(object sender, TE e);
}
