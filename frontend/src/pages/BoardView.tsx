import { useKeycloak } from '@react-keycloak/web';
import axios from 'axios';
import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';

interface List {
  id: string;
  boardId: string;
  title: string;
  position: number;
  colorArgb: number;
  cards: { id: string; title: string }[];
}

function BoardView() {
  const { boardId } = useParams<{ boardId: string }>();
  const { keycloak, initialized } = useKeycloak();
  const [lists, setLists] = useState<List[]>([]);

  useEffect(() => {
    if (initialized && keycloak.authenticated) {
      const fetchLists = async () => {
        try {
          const response = await axios.get(`/api/boards/${boardId}/lists`, {
            headers: { Authorization: `Bearer ${keycloak.token}` },
          });
          setLists(response.data);
        } catch (error) {
          console.error('Failed to fetch lists:', error);
        }
      };
      fetchLists();
    }
  }, [initialized, keycloak, boardId]);

  if (!initialized) return <div>Loading...</div>;

  return (
    <div>
      <h1>Board {boardId}</h1>
      <div style={{ display: 'flex', gap: '20px' }}>
        {lists
          .sort((a, b) => a.position - b.position)
          .map((list) => {
            // Convert ARGB int to CSS rgba string
            const argb = list.colorArgb.toString(16).padStart(8, '0');
            const a = parseInt(argb.slice(0, 2), 16) / 255;
            const r = parseInt(argb.slice(2, 4), 16);
            const g = parseInt(argb.slice(4, 6), 16);
            const b = parseInt(argb.slice(6, 8), 16);
            const bgColor = `rgba(${r},${g},${b},${a.toFixed(2)})`;
            return (
              <div
                key={list.id}
                style={{ border: '1px solid #ccc', padding: '10px', background: bgColor }}
              >
                <h3>{list.title}</h3>
                <ul>
                  {list.cards.map((card) => (
                    <li key={card.id}>{card.title}</li>
                  ))}
                </ul>
              </div>
            );
          })}
      </div>
    </div>
  );
}

export default BoardView;