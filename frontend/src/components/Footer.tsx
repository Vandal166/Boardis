import { useNavigate } from 'react-router-dom';

function Footer()
{
    const navigate = useNavigate();

    return (
        <footer className="w-full bg-gray-800 text-white py-12 shadow-2xl">
            <div className="container mx-auto px-6">
                <div className="grid grid-cols-1 md:grid-cols-4 gap-8">
                    <div>
                        <h3 className="text-xl font-semibold mb-4">About Boardis</h3>
                        <p className="text-sm">
                            Boardis is a modern Kanban app designed to help you organize your tasks and projects
                            efficiently.
                        </p>
                    </div>
                    <div>
                        <h3 className="text-xl font-semibold mb-4">Quick Links</h3>
                        <ul className="space-y-2 text-sm">
                            <li>
                                <button
                                    onClick={() => navigate('/')}
                                    className="hover:underline bg-transparent text-white text-left w-full"
                                >
                                    Home
                                </button>
                            </li>
                            <li>
                                <button
                                    onClick={() => navigate('/features')}
                                    className="hover:underline bg-transparent text-white text-left w-full"
                                >
                                    Features
                                </button>
                            </li>
                            <li>
                                <button
                                    onClick={() => navigate('/pricing')}
                                    className="hover:underline bg-transparent text-white text-left w-full"
                                >
                                    Pricing
                                </button>
                            </li>
                            <li>
                                <button
                                    onClick={() => navigate('/blog')}
                                    className="hover:underline bg-transparent text-white text-left w-full"
                                >
                                    Blog
                                </button>
                            </li>
                        </ul>
                    </div>
                    <div>
                        <h3 className="text-xl font-semibold mb-4">Support</h3>
                        <ul className="space-y-2 text-sm">
                            <li>
                                <button
                                    onClick={() => navigate('/help')}
                                    className="hover:underline bg-transparent text-white text-left w-full"
                                >
                                    Help Center
                                </button>
                            </li>
                            <li>
                                <button
                                    onClick={() => navigate('/contact')}
                                    className="hover:underline bg-transparent text-white text-left w-full"
                                >
                                    Contact Us
                                </button>
                            </li>
                            <li>
                                <button
                                    onClick={() => navigate('/privacy')}
                                    className="hover:underline bg-transparent text-white text-left w-full"
                                >
                                    Privacy Policy
                                </button>
                            </li>
                            <li>
                                <button
                                    onClick={() => navigate('/terms')}
                                    className="hover:underline bg-transparent text-white text-left w-full"
                                >
                                    Terms of Service
                                </button>
                            </li>
                        </ul>
                    </div>
                    <div>
                        <h3 className="text-xl font-semibold mb-4">Connect</h3>
                        <ul className="space-y-2 text-sm">
                            <li>
                                <a
                                    href="https://twitter.com/"
                                    target="_blank"
                                    rel="noopener noreferrer"
                                    className="hover:underline"
                                >
                                    Twitter
                                </a>
                            </li>
                            <li>
                                <a
                                    href="https://facebook.com/"
                                    target="_blank"
                                    rel="noopener noreferrer"
                                    className="hover:underline"
                                >
                                    Facebook
                                </a>
                            </li>
                            <li>
                                <a
                                    href="https://linkedin.com/"
                                    target="_blank"
                                    rel="noopener noreferrer"
                                    className="hover:underline"
                                >
                                    LinkedIn
                                </a>
                            </li>
                            <li>
                                <a
                                    href="https://instagram.com/"
                                    target="_blank"
                                    rel="noopener noreferrer"
                                    className="hover:underline"
                                >
                                    Instagram
                                </a>
                            </li>
                        </ul>
                    </div>
                </div>
                <div className="mt-8 text-center text-sm">
                    &copy; {new Date().getFullYear()} Boardis. All rights reserved.
                </div>
            </div>
        </footer>
    );
}

export default Footer;