import { useKeycloak } from '@react-keycloak/web';

function Settings() {
  const { keycloak, initialized } = useKeycloak();

  if (!initialized) return <div>Loading...</div>;

  if (!keycloak.authenticated) {
    keycloak.login({ redirectUri: window.location.origin + '/settings' });
    return null;
  }

  return (
    <div>
      <h1>Settings</h1>
      <p>Username: {keycloak.tokenParsed?.preferred_username || 'User'}</p>
      <button onClick={() => keycloak.logout({ redirectUri: window.location.origin + '/' })}>
        Logout
      </button>
    </div>
  );
}

export default Settings;