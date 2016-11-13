using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Api.Models
{

    public class Pedido
    {
        public string idPedido { get { return Guid.NewGuid().ToString(); } }
        public long idCliente { get; set; }
        public DateTime DataHora { get; set; }
        public List<itempedido> itens { get; set; }
        public decimal valortotal { get { return getVlrTotal(); } }

        public decimal getVlrTotal()
        {
            decimal tot = 0.00M;
            foreach (var item in this.itens)
                tot = tot + item.valor;
            return tot;

        }
    }

    public class itempedido
    {
        public long id { get; set; }
        public string descricao { get; set; }
        public decimal valor { get; set; }

        public itempedido(long ID, string Descricao, decimal Valor)
        {
            id = ID;
            descricao = Descricao;
            valor = Valor;
        }
    }



}