namespace ProyectoProgramacionIII.Models
{
    public class EstructurasLineales
    {
        public class NodoPila
        {
            public string Accion { get; set; }
            public NodoPila Siguiente { get; set; }

            public NodoPila(string accion)
            {
                Accion = accion;
                Siguiente = null;
            }
        }

        public class PilaHistorial
        {
            private NodoPila cima;

            public PilaHistorial()
            {
                cima = null;
            }
            public void Push(string accion)
            {
                NodoPila nuevo = new NodoPila(accion);
                nuevo.Siguiente = cima;
                cima = nuevo;
            }
            public string Pop()
            {
                if (cima == null)
                    throw new InvalidOperationException("La pila está vacía.");

                string accion = cima.Accion;
                cima = cima.Siguiente;
                return accion;
            }
            public string Peek()
            {
                if (cima == null)
                    throw new InvalidOperationException("La pila está vacía.");

                return cima.Accion;
            }

            public bool EstaVacia => cima == null;
        }

        public class NodoCola
        {
            public int IdArchivo { get; set; }
            public NodoCola Siguiente { get; set; }

            public NodoCola(int idArchivo)
            {
                IdArchivo = idArchivo;
                Siguiente = null;
            }
        }

        public class ColaDescargas
        {
            private NodoCola frente;
            private NodoCola final;

            public ColaDescargas()
            {
                frente = null;
                final = null;
            }

            public void Enqueue(int idArchivo)
            {
                NodoCola nuevo = new NodoCola(idArchivo);
                if (final != null)
                    final.Siguiente = nuevo;
                final = nuevo;
                if (frente == null)
                    frente = nuevo;
            }
            public int Dequeue()
            {
                if (frente == null)
                    throw new InvalidOperationException("La cola está vacía.");

                int id = frente.IdArchivo;
                frente = frente.Siguiente;
                if (frente == null)
                    final = null;
                return id;
            }

            public int Peek()
            {
                if (frente == null)
                    throw new InvalidOperationException("La cola está vacía.");

                return frente.IdArchivo;
            }

            public bool EstaVacia => frente == null;
        }
        public class AccionRequest
        {
            public string Accion { get; set; }
        }
        public class DescargaRequest
        {
            public int IdArchivo { get; set; }
        }
    }
}
