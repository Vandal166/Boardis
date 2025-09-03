import './App.css';
import { useKeycloak } from '@react-keycloak/web';
import { Outlet, Link } from 'react-router-dom';

function App() {
  const { initialized } = useKeycloak();

  return (
    <div>
      <nav style={{ marginBottom: 20 }}>
        <Link to="/">Home</Link> | <Link to="/settings">Settings</Link>
      </nav>
      {initialized ? (
        <Outlet /> // Render child routes (Home, BoardView, Settings)
      ) : (
        <div>
          <p>Loading Keycloak...</p>
          <Outlet />
        </div>
      )}
    </div>
  );
}

export default App;