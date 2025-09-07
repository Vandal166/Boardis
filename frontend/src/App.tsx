import './App.css';
import { useKeycloak } from '@react-keycloak/web';
import { Toaster } from 'react-hot-toast';
import { Outlet } from 'react-router-dom';

function App()
{
  const { initialized } = useKeycloak();

  return (
    <div>
      <Toaster position="top-right" />
      {initialized ? (
        <Outlet />
      ) : (
        <div>
          <Outlet />
        </div>
      )}
    </div>
  );
}

export default App;