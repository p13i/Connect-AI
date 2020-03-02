using System;
namespace Connect
{
    public class Grid<T>
    {
        private readonly int width;
        private readonly int height;

        private readonly T[,] grid;

        public Grid(int width, int height)
        {
            if (width < 1 || height < 1)
            {
                throw new ArgumentOutOfRangeException($"{nameof(width)} and {nameof(height)} must both be positive");
            }

            this.width = width;
            this.height = height;

            grid = new T[height, width];
            for (int x = 1; x <= width; x++)
            {
                for (int y = 1; y <= height; y++)
                {
                    this[x, y] = default;
                }
            }
        }

        public T this[int x, int y]
        {
            get
            {
                CheckBounds(x, y);
                return grid[height - y, x - 1];
            }
            set
            {
                CheckBounds(x, y);
                grid[height - y, x - 1] = value;
            }
        }

        private void CheckBounds(int x, int y)
        {
            if (x < 1 || x > width)
            {
                throw new ArgumentOutOfRangeException($"{nameof(x)}={x} must be between {1} and {width}");
            }

            if (y < 1 || y > height)
            {
                throw new ArgumentOutOfRangeException($"{nameof(y)}={y} must be between {1} and {height}");
            }
        }
    }
}
