using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using DevExpress.Xpf.Editors.Helpers;
using Shoko.Desktop.ViewModel.Helpers;

// ReSharper disable InconsistentNaming

namespace Shoko.Commons.Notification
{
    public static class INotifyPropertyChangedExtensions
    {
        public static void OnPropertyChanged(this INotifyPropertyChangedExt cls, Expression<Func<object>> selectorExpression)
        {
            var me = selectorExpression.Body as MemberExpression;
            if (me == null)
            {
                var ue = selectorExpression.Body as UnaryExpression;
                if (ue != null)
                    me = ue.Operand as MemberExpression;
            }
            if (me != null) cls.NotifyPropertyChanged(me.Member.Name);
        }

        public static T SetField<T>(this INotifyPropertyChangedExt cls, T field, T value, params Expression<Func<object>>[] props)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                props.ForEach(selectorExpression => OnPropertyChanged(cls, selectorExpression));
            }
            return value;
        }
        public static T SetField<T>(this INotifyPropertyChangedExt cls, T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                cls.NotifyPropertyChanged(propertyName);
            }
            return value;
        }
    }
}
