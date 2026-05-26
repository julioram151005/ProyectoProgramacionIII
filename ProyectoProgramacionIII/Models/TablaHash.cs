namespace ProyectoProgramacionIII.Models
{
    // Nodo para las listas enlazadas de cada bucket
    public class NodoHash
    {
        public string Clave { get; set; }
        public string Valor { get; set; }
        public NodoHash Siguiente { get; set; }

        public NodoHash(string clave, string valor)
        {
            Clave = clave;
            Valor = valor;
            Siguiente = null;
        }
    }

    public class TablaHash
    {
        private NodoHash[] buckets;
        private int capacidad;

        public TablaHash(int capacidadInicial = 101)
        {
            capacidad = capacidadInicial;
            buckets = new NodoHash[capacidad];
        }

        private int ObtenerIndice(string clave)
        {
            // Función hash simple (suma de códigos ASCII)
            int hash = 0;
            foreach (char c in clave)
                hash += c;
            return Math.Abs(hash % capacidad);
        }

        // Insertar o actualizar
        public void Insertar(string clave, string valor)
        {
            int indice = ObtenerIndice(clave);
            NodoHash actual = buckets[indice];

            // Buscar si la clave ya existe
            while (actual != null)
            {
                if (actual.Clave == clave)
                {
                    actual.Valor = valor; // Actualiza el valor
                    return;
                }
                actual = actual.Siguiente;
            }

            // Si no existe, insertar al inicio de la lista
            NodoHash nuevo = new NodoHash(clave, valor);
            nuevo.Siguiente = buckets[indice];
            buckets[indice] = nuevo;
        }

        // Buscar (devuelve el valor o null si no existe)
        public string Buscar(string clave)
        {
            int indice = ObtenerIndice(clave);
            NodoHash actual = buckets[indice];

            while (actual != null)
            {
                if (actual.Clave == clave)
                    return actual.Valor;
                actual = actual.Siguiente;
            }
            return null; // No encontrado
        }

        // Eliminar
        public bool Eliminar(string clave)
        {
            int indice = ObtenerIndice(clave);
            NodoHash actual = buckets[indice];
            NodoHash anterior = null;

            while (actual != null)
            {
                if (actual.Clave == clave)
                {
                    if (anterior == null)
                        buckets[indice] = actual.Siguiente; // Era el primer nodo
                    else
                        anterior.Siguiente = actual.Siguiente;
                    return true;
                }
                anterior = actual;
                actual = actual.Siguiente;
            }
            return false; // No encontrada
        }
    }
    public class HashEntryRequest
    {
        public string Clave { get; set; }
        public string Valor { get; set; }
    }
}
