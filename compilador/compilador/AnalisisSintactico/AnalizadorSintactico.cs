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
                MessageBox.Show("La compilacion ha finalizado exitosamente...");
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
        private void Expresion(int nivel)
        {
            FormarEntrada(nivel, "<Expresion>");
            Termino(nivel + 1);
            ExpresionPrima(nivel + 1);
            FormarSalida(nivel, "<Expresion>");
        }
        private void ExpresionPrima(int nivel)
        {
            FormarEntrada(nivel, "<ExpresionPrima>");
            if (Categoria.SUMA.Equals(Componente.ObtenerCategoria()))
            {
                Avanzar();
                Expresion(nivel+1);


            }
            else if (Categoria.RESTA.Equals(Componente.ObtenerCategoria()))
            {
                Avanzar();
                Expresion(nivel+1);
            }
            FormarSalida(nivel, "<ExpresionPrima>");
        }
        private void Termino(int nivel)
        {
            FormarEntrada(nivel, "<Termino>");
            Factor(nivel+1);
            TerminoPrima(nivel+1);
            FormarSalida(nivel, "<Termino>");
        }
        private void TerminoPrima(int nivel)
        {
            FormarEntrada(nivel, "<TerminoPrima>");
            if (Categoria.MULTIPLICACION.Equals(Componente.ObtenerCategoria()))
            {
                Avanzar();
                Termino(nivel+1);
                   

            } 
            else if (Categoria.DIVISION.Equals(Componente.ObtenerCategoria()))
            {
                Avanzar();
                Termino(nivel+1);
            }
            FormarSalida(nivel, "<TerminoPrima>");
        }
        private void Factor(int nivel)
        {
            FormarEntrada(nivel, "<Factor>");
            if (Categoria.NUMERO_ENTERO.Equals(Componente.ObtenerCategoria()))
            {
                Avanzar();
            }
            else if (Categoria.NUMERO_DECIMAL.Equals(Componente.ObtenerCategoria()))
            {
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
