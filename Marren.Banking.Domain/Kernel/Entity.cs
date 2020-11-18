using System;

namespace Marren.Banking.Domain.Kernel
{
    /// <summary>
    /// Representa uma entidade do domínio
    /// 
    /// Métodos de comparação e de hashcode foram sobrescritos 
    /// para levar em consideração o ID na comparação das instâncias
    /// de uma mesma entidade.
    /// </summary>
    public abstract class Entity
    {
        int? _requestedHashCode;

        /// <summary>
        /// O ID da entidade.
        /// Neste exemplo mais simples, o ID só pode ser inteiro.
        /// </summary>
        public virtual int Id 
        {
            get;
            protected set;
        }

        /// <summary>
        /// Indica se é uma instancia já persistida
        /// </summary>
        /// <returns></returns>
        public bool IsTransient()
        {
            return this.Id == default;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Entity))
                return false;

            if (Object.ReferenceEquals(this, obj))
                return true;

            if (this.GetType() != obj.GetType())
                return false;

            Entity item = (Entity)obj;

            if (item.IsTransient() || this.IsTransient())
                return false;
            else
                return item.Id == this.Id;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            if (!IsTransient())
            {
                if (!_requestedHashCode.HasValue)
                    _requestedHashCode = this.Id.GetHashCode() ^ 31; // XOR for random distribution (http://blogs.msdn.com/b/ericlippert/archive/2011/02/28/guidelines-and-rules-for-gethashcode.aspx)

                return _requestedHashCode.Value;
            }
            else
                return base.GetHashCode();

        }

        /// <summary>
        /// Compara duas instancias da entidade usando id da instância
        /// </summary>
        /// <param name="left">obj 1</param>
        /// <param name="right">obj 2</param>
        /// <returns>True se as instancias tem o mesmo ID</returns>
        public static bool operator == (Entity left, Entity right)
        {
            if (Object.Equals(left, null))
                return Object.Equals(right, null) ? true : false;
            else
                return left.Equals(right);
        }

        /// <summary>
        /// Compara duas instancias da entidade usando id da instância
        /// </summary>
        /// <param name="left">obj 1</param>
        /// <param name="right">obj 2</param>
        /// <returns>False se as instancias tem o mesmo ID</returns>
        public static bool operator != (Entity left, Entity right)
        {
            return !(left == right);
        }
    }
}
