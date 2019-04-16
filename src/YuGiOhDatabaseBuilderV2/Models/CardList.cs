using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace YuGiOhDatabaseBuilderV2.Models
{
    public class CardList : IList<Card>
    {
        private readonly List<Card> _cardList;

        public CardList()
        {
            _cardList = new List<Card>();
        }

        public Card this[int index] { get => _cardList[index]; set => throw new NotSupportedException(); }

        public int Count => _cardList.Count;

        public bool IsReadOnly => false;

        public void Add(Card card)
        {
            card.Id = GetLastIndex();
            _cardList.Add(card);

        }

        public void Clear() => _cardList.Clear();

        public bool Contains(Card card) => _cardList.Contains(card);

        public void CopyTo(Card[] array, int arrayIndex) => _cardList.CopyTo(array, arrayIndex);

        public IEnumerator<Card> GetEnumerator() => _cardList.GetEnumerator();

        public int IndexOf(Card card) => _cardList.IndexOf(card);

        public void Insert(int index, Card card) => _cardList.Insert(index, card);

        public bool Remove(Card card) => _cardList.Remove(card);

        public void RemoveAt(int index) => _cardList.RemoveAt(index);

        IEnumerator IEnumerable.GetEnumerator() => _cardList.GetEnumerator();

        public void AddRange(IEnumerable<Card> cards)
        {
            foreach (var card in cards) Add(card);
        }

        private int GetLastIndex()
        {
            if (_cardList.Count != (_cardList.LastOrDefault()?.Id ?? 0))
                return _cardList.LastOrDefault()?.Id + 1 ?? 0;

            return _cardList.Count;
        }
    }
}
