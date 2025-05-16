using System.Collections.Generic;

namespace Utilities.Generic
{
    public class UniqueList<T>
    {
        private readonly List<T> _list;
        private readonly IEqualityComparer<T> _comparer;

        public UniqueList(IEqualityComparer<T> _comparer = null)
        {
            _list = new List<T>();
            this._comparer = _comparer ?? EqualityComparer<T>.Default;
        }

        public bool Add(T _item)
        {
            if (_list.Exists(x => _comparer.Equals(x, _item)))
            {
                return false;
            }

            _list.Add(_item);
            return true;
        }

        public bool Remove(T _item)
        {
            if (_list.Exists(x => _comparer.Equals(x, _item)))
            {
                _list.Remove(_item);
                return true;
            }

            return false;
        }

        public int Count
        {
            get { return _list.Count; }
        }

        public T this[int t]
        {
            get { return _list[t]; }
            set 
            {
                if (!_list.Contains(value))
                    _list[t] = value; 
            }
        }
    }
}
