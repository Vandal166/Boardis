import React from 'react';

interface BoardCard
{
    id: string;
    boardId: string;
    boardListId: string;
    title: string;
    description?: string;
    completedAt?: string;
    position: number;
}

interface BoardCardsProps
{
    cards: BoardCard[];
}

const BoardCards: React.FC<BoardCardsProps> = ({ cards }) =>
{
    if (!cards || cards.length === 0) return null;

    return (
        <ul className="space-y-3 flex-1">
            {cards.map((card) => (
                <li
                    key={card.id}
                    className="bg-white p-3 rounded shadow-md"
                >
                    <h4 className="font-medium text-blue-800">{card.title}</h4>
                    {card.description && (
                        <p className="text-sm text-gray-600 mt-1">{card.description}</p>
                    )}
                    {card.completedAt && (
                        <p className="text-xs text-green-600 mt-1">
                            Completed: {new Date(card.completedAt).toLocaleString()}
                        </p>
                    )}
                </li>
            ))}
        </ul>
    );
};

export default BoardCards;