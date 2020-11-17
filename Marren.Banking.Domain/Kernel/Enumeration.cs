using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Marren.Banking.Domain.Kernel
{
    /// <summary>
    /// Classe para representar as enumerações de domínio
    /// 
    /// É diferente de usar uma enum do C# por que permite extender
    /// colocando outros campos como descrição e outros.
    /// 
    /// Pode ter uma tabela no repositório associada a aela.
    /// </summary>
    public abstract class Enumeration : IComparable
    {
        /// <summary>
        /// Nome do item 
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Identificador
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="name">nome</param>
        protected Enumeration(int id, string name)
        {
            Id = id;
            Name = name;
        }

        /// <inheritdoc/>
        public override string ToString() => Name;

        /// <summary>
        /// Obtém todos os valores possíveis desta enumeração
        /// </summary>
        /// <typeparam name="T">Tipo da Enumeração</typeparam>
        /// <returns></returns>
        public static IEnumerable<T> GetAll<T>() where T : Enumeration
        {
            var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

            return fields.Select(f => f.GetValue(null)).Cast<T>();
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var otherValue = obj as Enumeration;

            if (otherValue == null)
                return false;

            var typeMatches = GetType().Equals(obj.GetType());
            var valueMatches = Id.Equals(otherValue.Id);

            return typeMatches && valueMatches;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => Id.GetHashCode();

        /// <summary>
        /// Compara este enum com outro
        /// </summary>
        /// <param name="other">outro enum</param>
        /// <returns>Comparação entre IDs das enums</returns>
        public int CompareTo(object other) => Id.CompareTo(((Enumeration)other).Id);

        /// <summary>
        /// Busca um item por id
        /// </summary>
        /// <typeparam name="T">Tipo da enum</typeparam>
        /// <param name="id">id</param>
        /// <returns>Item da enum</returns>
        public static T FromId<T>(int id) where T : Enumeration
        {
            return GetAll<T>().FirstOrDefault(x => x.Id == id);
        }
    }
}