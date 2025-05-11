using System;

namespace Proyecto
{
    public class MiLista<T>
    {
        private T[] elementos;
        private int cantidad;

        public MiLista()
        {
            elementos = new T[10];
            cantidad = 0;
        }

        public void Add(T item)
        {
            if (cantidad == elementos.Length)
            {
                T[] nuevo = new T[elementos.Length * 2];
                for (int i = 0; i < elementos.Length; i++)
                    nuevo[i] = elementos[i];
                elementos = nuevo;
            }
            elementos[cantidad++] = item;
        }

        public void Clear()
        {
            cantidad = 0;
        }

        public int Count => cantidad;

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= cantidad)
                    throw new IndexOutOfRangeException();
                return elementos[index];
            }
        }

        public T[] ToArray()
        {
            T[] copia = new T[cantidad];
            for (int i = 0; i < cantidad; i++)
                copia[i] = elementos[i];
            return copia;
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            for (int i = 0; i < cantidad; i++)
                yield return elementos[i];
        }
    }
}
