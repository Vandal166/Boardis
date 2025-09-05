import { useKeycloak } from '@react-keycloak/web';
import Header from '../components/Header';
import Footer from '../components/Footer';
import HomeContent from '../components/HomeContent';


function Home() {
  useKeycloak();


  return (
    <div className="min-h-screen text-gray-900 font-sans">
      <Header />
      
      <HomeContent />
      
      <Footer />
    </div>
  );
}

export default Home;