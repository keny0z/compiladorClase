using compilador.AnalisisLexico;
using compilador.ManejadorErrores;
using compilador.Transversal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace compilador.AnalisisSintactico
{
    public class AnalizadorSintactico
    {
        private AnalizadorLexico AnaLex = new AnalizadorLexico();
        private ComponenteLexico Componente;
        private StringBuilder Traza;
        private Stack<double> Pila = new Stack<double>();

        public void Analizar(bool Depurar)
        {
            Avanzar();
            Traza = new StringBuilder();
            Expresion(1);

            if (Depurar)
            {
                MessageBox.Show(Traza.ToString());
            }
            if (GestorErrores.ObtenerInstancia().HayErrores())
            {
                MessageBox.Show("La compilacion ha finalizado, pero hay errores en el programa de entrada. Por favor verifique la consola de errores...");
            }
            else if (Categoria.FIN_ARCHIVO.Equals(Componente.ObtenerCategoria()))
            {
                if(Pila.Count == 1)
                {
                    MessageBox.Show("La compilacion ha finalizado exitosamente...");
                    MessageBox.Show("Resultado: "+ Pila.Pop());
                }
                else
                {
                    MessageBox.Show("Aunque la compilacion ha finalizado exitosamente, faltaron componentes por evaluar.");
                }
               
            }
            else
            {
                MessageBox.Show("Aunque la compilacion ha finalizado exitosamente, faltaron componentes por evaluar.");
            }
        }

        private void Avanzar()
        {
            Componente = AnaLex.DevolverComponenteLexico();
        }

        private void FormarEntrada(int Nivel,String Regla)
        {
            
            for(int Contador =1; Contador <= Nivel * 5; Contador++)
            {
                Traza.Append("-");
            }
            Traza.Append(">");
            Traza.Append("INICIO ").Append(Regla);
            Traza.Append("\n");

            FormarComponente(Nivel);

            
        }
        private void FormarSalida(int Nivel, String Regla)
        {
            Traza.Append("<");
            for (int Contador = 1; Contador <= Nivel * 5; Contador++)
            {
                Traza.Append("-");
            }

            Traza.Append("FIN ").Append(Regla);
            Traza.Append("\n");

            
        }
        private void FormarComponente(int Nivel)
        {
            Traza.Append("-");
            for (int Contador = 1; Contador <= (Nivel+1) * 5; Contador++)
            {
                Traza.Append("-");
            }

            Traza.Append("Componente actual: ").Append(this.Componente.ObtenerLexema()).Append("/").Append(this.Componente.ObtenerCategoria());
            Traza.Append("\n");

        }
        private void FormarOperacion(int Nivel, String operacion)
        {
            Traza.Append("-");
            for (int Contador = 1; Contador <= (Nivel + 1) * 5; Contador++)
            {
                Traza.Append("-");
            }

            Traza.Append("Operando: ").Append(operacion).Append("\n");
            

        }
        //<Expresion> -> <Termino><ExpresionPrima>
        private void Expresion(int nivel)
        {
            FormarEntrada(nivel, "<Expresion>");
            Termino(nivel + 1);
            ExpresionPrima(nivel + 1);
            FormarSalida(nivel, "<Expresion>");
        }
        //<ExpresionPrima> -> +<Expresion> {PUSH({POP-2}+{POP-1})}| -<Expresion>{PUSH({POP-2}-{POP-1})} | Epsilon
        private void ExpresionPrima(int nivel)
        {
            FormarEntrada(nivel, "<ExpresionPrima>");
            if (Categoria.SUMA.Equals(Componente.ObtenerCategoria()))
            {
                Avanzar();
                Expresion(nivel+1);

                if (!GestorErrores.ObtenerInstancia().HayErrores())
                {
                    Double Derecho = Pila.Pop();
                    Double Izquierdo = Pila.Pop();
                    Pila.Push(Izquierdo + Derecho);
                    FormarOperacion(nivel, Izquierdo + " + " + Derecho);
                }

            }
            else if (Categoria.RESTA.Equals(Componente.ObtenerCategoria()))
            {
                Avanzar();
                Expresion(nivel+1);

                if (!GestorErrores.ObtenerInstancia().HayErrores())
                {
                    Double Derecho = Pila.Pop();
                    Double Izquierdo = Pila.Pop();
                    Pila.Push(Izquierdo - Derecho);
                    FormarOperacion(nivel, Izquierdo + " - " + Derecho);
                }
            }
            FormarSalida(nivel, "<ExpresionPrima>");
        }
        //<Termino> -> <Factor><TerminoPrima>
        private void Termino(int nivel)
        {
            FormarEntrada(nivel, "<Termino>");
            Factor(nivel+1);
            TerminoPrima(nivel+1);
            FormarSalida(nivel, "<Termino>");
        }
        //<TerminoPrima> -> *<Termino>{PUSH({POP-2}*{POP-1})} | /<Termino>{PUSH({POP-2}/{POP-1})} | Epsilon
        private void TerminoPrima(int nivel)
        {
            FormarEntrada(nivel, "<TerminoPrima>");
            if (Categoria.MULTIPLICACION.Equals(Componente.ObtenerCategoria()))
            {
                Avanzar();
                Termino(nivel+1);

                if (!GestorErrores.ObtenerInstancia().HayErrores())
                {
                    Double Derecho = Pila.Pop();
                    Double Izquierdo = Pila.Pop();
                    Pila.Push(Izquierdo * Derecho);
                    FormarOperacion(nivel, Izquierdo + " * " + Derecho);
                }
                   

            } 
            else if (Categoria.DIVISION.Equals(Componente.ObtenerCategoria()))
            {
                Avanzar();
                Termino(nivel+1);

                if (!GestorErrores.ObtenerInstancia().HayErrores())
                {
                    Double Derecho = Pila.Pop();
                    Double Izquierdo = Pila.Pop();
                    FormarOperacion(nivel, Izquierdo + " / " + Derecho);

                    if (Derecho == 0)
                    {
                        string Falla = "Operacion no valida: Division por cero";
                        string Causa = "El denominador es cero";
                        string Solucion = "Asegúrese de que el denominador sea diferente de cero";


                        Error Error = ManejadorErrores.Error.Crear(Componente.ObtenerNumeroLinea(), Componente.ObtenerPosicionInicial(), Componente.ObtenerPosicionFinal(), Falla, Causa, Solucion, ManejadorErrores.TipoError.SEMANTICO);
                        GestorErrores.ObtenerInstancia().Agregar(Error);
                    }
                    else
                    {
                        Pila.Push(Izquierdo / Derecho);
                    }
                }
            }
            FormarSalida(nivel, "<TerminoPrima>");
        }
        //<Factor> -> NUMERO ENTERO{PUSH} | NUMERO DECIMAL{PUSH} | (<Expresion>)
        private void Factor(int nivel)
        {
            FormarEntrada(nivel, "<Factor>");
            if (Categoria.NUMERO_ENTERO.Equals(Componente.ObtenerCategoria()))
            {
                Pila.Push(Convert.ToInt32(Componente.ObtenerLexema()));
                Avanzar();
            }
            else if (Categoria.NUMERO_DECIMAL.Equals(Componente.ObtenerCategoria()))
            {
                Pila.Push(Convert.ToDouble(Componente.ObtenerLexema()));
                Avanzar();
            }
            else if (Categoria.PARENTESIS_ABRE.Equals(Componente.ObtenerCategoria()))
            {
                Avanzar();
                Expresion(nivel+1);
                if (Categoria.PARENTESIS_CIERRA.Equals(Componente.ObtenerCategoria()))
                {
                    Avanzar();
                }
                else
                {
                    string Falla = "Componente no valido: " + Componente.ObtenerLexema();
                    string Causa = "Recibí " + Componente.ObtenerCategoria() + " y esperaba ) (PARENTESIS_CIERRA)";
                    string Solucion = "Asegurese de que el parentesis que cierra este ubicado en esta posicion";

                    Error Error = ManejadorErrores.Error.Crear(Componente.ObtenerNumeroLinea(), Componente.ObtenerPosicionInicial(), Componente.ObtenerPosicionFinal(), Falla, Causa, Solucion, TipoError.SINTACTICO);
                    GestorErrores.ObtenerInstancia().Agregar(Error);
                }
            }
            else
            {
                string Falla = "Componente no válido: " + Componente.ObtenerLexema();
                string Causa = "Recibí " + Componente.ObtenerCategoria() + " ...";
                string Solucion = "Asegúrese de que el componente que está en esta posición sea un número entero, decimal o paréntesis abre...";


                Error Error = ManejadorErrores.Error.Crear(Componente.ObtenerNumeroLinea(), Componente.ObtenerPosicionInicial(), Componente.ObtenerPosicionFinal(), Falla, Causa, Solucion, ManejadorErrores.TipoError.SINTACTICO);
                GestorErrores.ObtenerInstancia().Agregar(Error);

                throw new Exception("Error tipo stopper durante el análisis sintáctico. Por favor verifique la consola de errores...");
            }
            FormarSalida(nivel, "<Factor>");

        }
    }
}
