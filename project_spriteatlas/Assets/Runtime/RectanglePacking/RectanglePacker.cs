
using System.Collections.Generic;

namespace SpriteAtlas
{
    public class RectanglePacker 
	{
		private int _width = 0;
		private int _height = 0;
        private int _cellWidth = 0;
        private int _cellHeight = 0;
        private int _padding = 1;

		private List<IntegerRectangle> _freeRectangles;
        private List<IntegerRectangle> _usedRectangles;

        public RectanglePacker(int width, int height, int cellWidth, int cellHeight, int padding = 0) 
		{
            _width = width;
			_height = height;
			_cellWidth = cellWidth;
			_cellHeight = cellHeight;
			_padding = padding;

			Reset();
        }
		private void Reset()
		{
			int w = _cellWidth + _padding;
			int h = _cellHeight + _padding;
			int rows = _height / h;
			int cols = _width / w;
			_freeRectangles = new List<IntegerRectangle>(rows * cols);
			for(int row = 0; row < rows; ++row)
			{
                for (int col = 0; col < cols; ++col)
                {
					var rect = new IntegerRectangle(col * w, row * h, _cellWidth, _cellHeight);
					_freeRectangles.Add(rect);
                }
            }
			_usedRectangles = new List<IntegerRectangle>();
        }
        public bool TryAddRectangle(string name, out IntegerRectangle rect)
        {
			for(int i = 0; i < _usedRectangles.Count; ++i)
			{
				if (_usedRectangles[i].name == name)
				{
					rect = _usedRectangles[i];
					return true;
				}
			}
            if (TryGetFreeRectangle(out rect))
            {
				rect.name = name;
                _usedRectangles.Add(rect);
                return true;
            }
            return false;
        }
		public bool RemoveRectangle(string name) 
		{
			for(int i = 0; i < _usedRectangles.Count; ++i)
			{
				if (_usedRectangles[i].name == name)
				{
                    _usedRectangles[i].name = "";
                    _freeRectangles.Add(_usedRectangles[i]);
					_usedRectangles.RemoveAt(i);
					return true;
				}
			}
			return false;
		}

        public int GetRectangleCount()
        {
            return _usedRectangles.Count;
        }
		public int GetFreeCount()
		{
			return _freeRectangles.Count;
		}
		public bool HasRectangle(string name)
		{
            for (int i = 0; i < _usedRectangles.Count; ++i)
            {
                if (_usedRectangles[i].name == name)
                {
                    return true;
                }
            }
			return false;
        }

        private bool TryGetFreeRectangle(out IntegerRectangle rect)
		{
			if(_freeRectangles.Count == 0)
			{
				rect = null;
                return false;
            }
			rect = _freeRectangles[_freeRectangles.Count - 1];
			_freeRectangles.RemoveAt(_freeRectangles.Count - 1);
			return true;
		}
    }
}