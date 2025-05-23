using System;
using System.Collections.Generic;
using System.Linq;

namespace Hospedagem
{
    enum TipoHospede
    {
        Adulto,
        Crianca
    }

    class Pessoa
    {
        public string Nome { get; }
        public int Idade { get; }
        public TipoHospede Tipo { get; }

        public Pessoa(string nome, int idade)
        {
            if (string.IsNullOrWhiteSpace(nome)) throw new ArgumentException("Nome inválido");
            if (idade < 0) throw new ArgumentException("Idade inválida");

            Nome = nome;
            Idade = idade;
            Tipo = idade < 12 ? TipoHospede.Crianca : TipoHospede.Adulto;
        }
    }

    class Suite
    {
        public string Nome { get; }
        public int CapacidadeAdultos { get; }
        public int CapacidadeCriancas { get; }
        public decimal ValorAdulto { get; }
        public decimal ValorCrianca { get; }
        public bool Disponivel { get; private set; } = true;

        public Suite(string nome, int capAdultos, int capCriancas, decimal valorAdulto, decimal valorCrianca)
        {
            if (string.IsNullOrWhiteSpace(nome)) throw new ArgumentException("Nome inválido");
            if (capAdultos < 1) throw new ArgumentException("Capacidade adultos inválida");
            if (capCriancas < 0) throw new ArgumentException("Capacidade crianças inválida");
            if (valorAdulto <= 0) throw new ArgumentException("Valor adulto inválido");
            if (valorCrianca < 0) throw new ArgumentException("Valor criança inválido");

            Nome = nome;
            CapacidadeAdultos = capAdultos;
            CapacidadeCriancas = capCriancas;
            ValorAdulto = valorAdulto;
            ValorCrianca = valorCrianca;
        }

        public void SetDisponibilidade(bool status)
        {
            Disponivel = status;
        }
    }

    class Reserva
    {
        private List<Pessoa> hospedes = new List<Pessoa>();
        public IReadOnlyList<Pessoa> Hospedes => hospedes.AsReadOnly();

        public Suite Suite { get; }
        public int Dias { get; }
        public bool Cancelada { get; private set; } = false;

        const decimal TaxaLimpeza = 50m;

        public Reserva(Suite suite, int dias)
        {
            if (suite == null) throw new ArgumentNullException(nameof(suite));
            if (!suite.Disponivel) throw new InvalidOperationException("Suíte indisponível");
            if (dias <= 0) throw new ArgumentException("Dias inválidos");

            Suite = suite;
            Dias = dias;
        }

        public void AdicionarHospedes(List<Pessoa> lista)
        {
            if (Cancelada) throw new InvalidOperationException("Reserva cancelada");
            if (lista == null || lista.Count == 0) throw new ArgumentException("Lista vazia");

            int adultos = lista.Count(p => p.Tipo == TipoHospede.Adulto);
            int criancas = lista.Count(p => p.Tipo == TipoHospede.Crianca);

            if (adultos > Suite.CapacidadeAdultos)
                throw new InvalidOperationException("Excedeu capacidade adultos");
            if (criancas > Suite.CapacidadeCriancas)
                throw new InvalidOperationException("Excedeu capacidade crianças");

            hospedes = new List<Pessoa>(lista);
        }

        public decimal CalcularValorDiaria()
        {
            decimal totalAdultos = hospedes.Where(p => p.Tipo == TipoHospede.Adulto).Sum(p => Suite.ValorAdulto);
            decimal totalCriancas = hospedes.Where(p => p.Tipo == TipoHospede.Crianca).Sum(p => Suite.ValorCrianca);
            return totalAdultos + totalCriancas;
        }

        public decimal CalcularValorTotal()
        {
            if (Cancelada) return 0;

            decimal valorBase = CalcularValorDiaria() * Dias + TaxaLimpeza;

            decimal desconto = 0;
            if (Dias > 15) desconto = 0.2m;
            else if (Dias > 10) desconto = 0.1m;

            return valorBase * (1 - desconto);
        }

        public void Cancelar()
        {
            Cancelada = true;
            hospedes.Clear();
            Suite.SetDisponibilidade(true);
        }

        public void MostrarHospedes()
        {
            if (!hospedes.Any())
            {
                Console.WriteLine("Sem hóspedes na reserva");
                return;
            }

            foreach (var p in hospedes)
                Console.WriteLine($"{p.Nome} ({p.Tipo}, {p.Idade} anos)");
        }
    }

    class Program
    {
        static void Main()
        {
            try
            {
                var suite = new Suite("Executiva", 2, 1, 400m, 200m);
                var hospedes = new List<Pessoa>
                {
                    new Pessoa("Lucas", 32),
                    new Pessoa("Ana", 29),
                    new Pessoa("Pedro", 8)
                };

                var reserva = new Reserva(suite, 12);
                reserva.AdicionarHospedes(hospedes);

                Console.WriteLine($"Hóspedes: {reserva.Hospedes.Count}");
                reserva.MostrarHospedes();
                Console.WriteLine($"Valor total: R$ {reserva.CalcularValorTotal():F2}");

                reserva.Cancelar();
                Console.WriteLine("Reserva cancelada.");
                Console.WriteLine($"Valor após cancelamento: R$ {reserva.CalcularValorTotal():F2}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro: " + ex.Message);
            }
        }
    }
}
