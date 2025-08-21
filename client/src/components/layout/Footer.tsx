export function Footer() {
  return (
    <footer className="bg-white dark:bg-gray-900 border-t border-gray-200 dark:border-gray-800">
      <div className="max-w-7xl mx-auto py-12 px-4 sm:px-6 lg:px-8">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-8">
          <div>
            <div className="flex items-center mb-4">
              <i className="fas fa-code text-2xl text-brand-600 mr-2"></i>
              <span className="text-xl font-bold text-gray-900 dark:text-white">DSAGrind</span>
            </div>
            <p className="text-gray-600 dark:text-gray-400 text-sm">
              Master data structures and algorithms with AI-powered insights and real-time feedback.
            </p>
          </div>
          
          <div>
            <h3 className="text-sm font-semibold text-gray-900 dark:text-white uppercase tracking-wider mb-4">
              Practice
            </h3>
            <ul className="space-y-2">
              <li>
                <a href="/problems" className="text-gray-600 dark:text-gray-400 hover:text-brand-600 text-sm">
                  Problems
                </a>
              </li>
              <li>
                <a href="/contests" className="text-gray-600 dark:text-gray-400 hover:text-brand-600 text-sm">
                  Contests
                </a>
              </li>
              <li>
                <a href="/interview" className="text-gray-600 dark:text-gray-400 hover:text-brand-600 text-sm">
                  Interview Prep
                </a>
              </li>
            </ul>
          </div>
          
          <div>
            <h3 className="text-sm font-semibold text-gray-900 dark:text-white uppercase tracking-wider mb-4">
              Community
            </h3>
            <ul className="space-y-2">
              <li>
                <a href="/discuss" className="text-gray-600 dark:text-gray-400 hover:text-brand-600 text-sm">
                  Discussions
                </a>
              </li>
              <li>
                <a href="/help" className="text-gray-600 dark:text-gray-400 hover:text-brand-600 text-sm">
                  Help Center
                </a>
              </li>
              <li>
                <a href="/feedback" className="text-gray-600 dark:text-gray-400 hover:text-brand-600 text-sm">
                  Feedback
                </a>
              </li>
            </ul>
          </div>
          
          <div>
            <h3 className="text-sm font-semibold text-gray-900 dark:text-white uppercase tracking-wider mb-4">
              Company
            </h3>
            <ul className="space-y-2">
              <li>
                <a href="/about" className="text-gray-600 dark:text-gray-400 hover:text-brand-600 text-sm">
                  About
                </a>
              </li>
              <li>
                <a href="/privacy" className="text-gray-600 dark:text-gray-400 hover:text-brand-600 text-sm">
                  Privacy Policy
                </a>
              </li>
              <li>
                <a href="/terms" className="text-gray-600 dark:text-gray-400 hover:text-brand-600 text-sm">
                  Terms of Service
                </a>
              </li>
            </ul>
          </div>
        </div>
        
        <div className="mt-8 pt-8 border-t border-gray-200 dark:border-gray-800">
          <p className="text-center text-gray-500 dark:text-gray-400 text-sm">
            © 2024 DSAGrind. All rights reserved. Built with ❤️ for developers.
          </p>
        </div>
      </div>
    </footer>
  );
}
