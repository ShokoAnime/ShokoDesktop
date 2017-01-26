using System.Collections.Generic;

namespace Shoko.Desktop
{
    public interface IListWrapper
    {
        int ObjectType { get; }
        bool IsEditable { get; }
        List<IListWrapper> GetDirectChildren();
    }
}
