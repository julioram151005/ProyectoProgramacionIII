namespace ProyectoProgramacionIII.Models
{
    public class NodoArbol
    {
        public int Valor { get; set; }
        public NodoArbol Izquierdo { get; set; }
        public NodoArbol Derecho { get; set; }

        public NodoArbol(int valor)
        {
            Valor = valor;
            Izquierdo = null;
            Derecho = null;
        }
    }

    public class ArbolBinario
    {
        public NodoArbol Raiz { get; set; }

        public ArbolBinario()
        {
            Raiz = null;
        }

        public void Insertar(int valor)
        {
            Raiz = InsertarRecursivo(Raiz, valor);
        }

        private NodoArbol InsertarRecursivo(NodoArbol nodo, int valor)
        {
            if (nodo == null)
                return new NodoArbol(valor);

            if (valor < nodo.Valor)
                nodo.Izquierdo = InsertarRecursivo(nodo.Izquierdo, valor);
            else if (valor > nodo.Valor)
                nodo.Derecho = InsertarRecursivo(nodo.Derecho, valor);

            return nodo;
        }

        // Método público de eliminación
        public void Eliminar(int valor)
        {
            Raiz = EliminarRecursivo(Raiz, valor);
        }

        private NodoArbol EliminarRecursivo(NodoArbol nodo, int valor)
        {
            if (nodo == null) return null;

            if (valor < nodo.Valor)
                nodo.Izquierdo = EliminarRecursivo(nodo.Izquierdo, valor);
            else if (valor > nodo.Valor)
                nodo.Derecho = EliminarRecursivo(nodo.Derecho, valor);
            else
            {
                // Nodo con un solo hijo o sin hijos
                if (nodo.Izquierdo == null)
                    return nodo.Derecho;
                if (nodo.Derecho == null)
                    return nodo.Izquierdo;

                // Nodo con dos hijos: sucesor en orden
                NodoArbol sucesor = EncontrarMinimo(nodo.Derecho);
                nodo.Valor = sucesor.Valor;
                nodo.Derecho = EliminarRecursivo(nodo.Derecho, sucesor.Valor);
            }
            return nodo;
        }

        private NodoArbol EncontrarMinimo(NodoArbol nodo)
        {
            while (nodo.Izquierdo != null)
                nodo = nodo.Izquierdo;
            return nodo;
        }

        // Recorridos
        public List<int> PreOrden()
        {
            var resultado = new List<int>();
            PreOrdenRecursivo(Raiz, resultado);
            return resultado;
        }

        private void PreOrdenRecursivo(NodoArbol nodo, List<int> resultado)
        {
            if (nodo != null)
            {
                resultado.Add(nodo.Valor);
                PreOrdenRecursivo(nodo.Izquierdo, resultado);
                PreOrdenRecursivo(nodo.Derecho, resultado);
            }
        }

        public List<int> InOrden()
        {
            var resultado = new List<int>();
            InOrdenRecursivo(Raiz, resultado);
            return resultado;
        }

        private void InOrdenRecursivo(NodoArbol nodo, List<int> resultado)
        {
            if (nodo != null)
            {
                InOrdenRecursivo(nodo.Izquierdo, resultado);
                resultado.Add(nodo.Valor);
                InOrdenRecursivo(nodo.Derecho, resultado);
            }
        }

        public List<int> PostOrden()
        {
            var resultado = new List<int>();
            PostOrdenRecursivo(Raiz, resultado);
            return resultado;
        }

        private void PostOrdenRecursivo(NodoArbol nodo, List<int> resultado)
        {
            if (nodo != null)
            {
                PostOrdenRecursivo(nodo.Izquierdo, resultado);
                PostOrdenRecursivo(nodo.Derecho, resultado);
                resultado.Add(nodo.Valor);
            }
        }
    }
}