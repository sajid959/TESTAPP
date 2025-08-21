import { Link } from 'wouter';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { useAuth } from '@/hooks/useAuth';

export default function Home() {
  const { user } = useAuth();

  const stats = [
    { label: 'Coding Problems', value: '2,500+', color: 'text-brand-600' },
    { label: 'Active Users', value: '50K+', color: 'text-success-600' },
    { label: 'Languages', value: '15', color: 'text-purple-600' },
    { label: 'Submissions', value: '1M+', color: 'text-warning-600' },
  ];

  const features = [
    {
      icon: 'fas fa-robot',
      title: 'AI-Powered Hints',
      description: 'Get intelligent hints and debugging suggestions powered by Perplexity AI.',
      color: 'bg-blue-50 dark:bg-blue-900/20 text-blue-600',
    },
    {
      icon: 'fas fa-code',
      title: 'Multi-Language IDE',
      description: 'Practice in Python, Java, C++, JavaScript, and 11 more programming languages.',
      color: 'bg-success-50 dark:bg-success-900/20 text-success-600',
    },
    {
      icon: 'fas fa-chart-line',
      title: 'Progress Tracking',
      description: 'Track your solving progress, streaks, and improvement over time.',
      color: 'bg-purple-50 dark:bg-purple-900/20 text-purple-600',
    },
    {
      icon: 'fas fa-users',
      title: 'Community Discussions',
      description: 'Learn from others and share your solutions with the coding community.',
      color: 'bg-warning-50 dark:bg-warning-900/20 text-warning-600',
    },
    {
      icon: 'fas fa-brain',
      title: 'Interview Prep',
      description: 'Practice problems from top tech companies to ace your coding interviews.',
      color: 'bg-pink-50 dark:bg-pink-900/20 text-pink-600',
    },
    {
      icon: 'fas fa-crown',
      title: 'Premium Features',
      description: 'Unlock advanced problems, detailed explanations, and exclusive content.',
      color: 'bg-amber-50 dark:bg-amber-900/20 text-amber-600',
    },
  ];

  return (
    <div className="min-h-screen">
      {/* Hero Section */}
      <section className="hero-gradient py-20" data-testid="hero-section">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center">
            <h1 className="text-5xl font-bold text-slate-900 dark:text-white mb-6 animate-fade-in">
              Master Data Structures & 
              <span className="text-brand-600 block">Algorithms</span>
            </h1>
            <p className="text-xl text-slate-600 dark:text-slate-400 mb-8 max-w-3xl mx-auto animate-fade-in" style={{ animationDelay: '0.2s' }}>
              Practice coding problems, prepare for interviews, and level up your programming skills with AI-powered insights and real-time feedback.
            </p>
            <div className="flex flex-col sm:flex-row justify-center gap-4 animate-slide-up" style={{ animationDelay: '0.4s' }}>
              <Link href="/problems">
                <Button size="lg" className="bg-brand-600 hover:bg-brand-700 text-lg px-8 py-4 rounded-xl" data-testid="button-start-practicing">
                  <i className="fas fa-play mr-2"></i>
                  Start Practicing
                </Button>
              </Link>
              {!user?.isPremium && (
                <Link href="/subscribe">
                  <Button variant="outline" size="lg" className="text-lg px-8 py-4 rounded-xl border-2 border-brand-600 text-brand-600 hover:bg-brand-50 dark:hover:bg-brand-950" data-testid="button-view-premium">
                    <i className="fas fa-crown mr-2"></i>
                    View Premium
                  </Button>
                </Link>
              )}
            </div>
          </div>
          
          {/* Stats */}
          <div className="grid grid-cols-1 md:grid-cols-4 gap-8 mt-16 animate-slide-up" style={{ animationDelay: '0.6s' }}>
            {stats.map((stat, index) => (
              <Card key={stat.label} className="bg-white/80 dark:bg-slate-900/80 backdrop-blur-sm shadow-lg">
                <CardContent className="p-6 text-center">
                  <div className={`text-3xl font-bold ${stat.color} mb-2`}>
                    {stat.value}
                  </div>
                  <div className="text-slate-600 dark:text-slate-400">{stat.label}</div>
                </CardContent>
              </Card>
            ))}
          </div>
        </div>
      </section>

      {/* Features Section */}
      <section className="py-20 bg-white dark:bg-slate-900" data-testid="features-section">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-16">
            <h2 className="text-3xl font-bold text-slate-900 dark:text-white mb-4">
              Why Choose DSAGrind?
            </h2>
            <p className="text-xl text-slate-600 dark:text-slate-400 max-w-3xl mx-auto">
              Everything you need to master coding interviews and become a better programmer.
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
            {features.map((feature, index) => (
              <Card key={feature.title} className="hover:shadow-lg transition-shadow duration-300">
                <CardContent className="p-6">
                  <div className={`inline-flex items-center justify-center w-12 h-12 rounded-lg ${feature.color} mb-4`}>
                    <i className={`${feature.icon} text-lg`}></i>
                  </div>
                  <h3 className="text-xl font-semibold text-slate-900 dark:text-white mb-3">
                    {feature.title}
                  </h3>
                  <p className="text-slate-600 dark:text-slate-400">
                    {feature.description}
                  </p>
                </CardContent>
              </Card>
            ))}
          </div>
        </div>
      </section>

      {/* Popular Categories */}
      <section className="py-20 bg-slate-50 dark:bg-slate-800" data-testid="categories-section">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-16">
            <h2 className="text-3xl font-bold text-slate-900 dark:text-white mb-4">
              Popular Problem Categories
            </h2>
            <p className="text-xl text-slate-600 dark:text-slate-400">
              Start with these fundamental topics to build a strong foundation.
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {[
              { name: 'Arrays & Hashing', count: 125, difficulty: 'Beginner Friendly', color: 'bg-blue-500' },
              { name: 'Two Pointers', count: 45, difficulty: 'Intermediate', color: 'bg-success-500' },
              { name: 'Sliding Window', count: 32, difficulty: 'Intermediate', color: 'bg-purple-500' },
              { name: 'Stack', count: 38, difficulty: 'Beginner Friendly', color: 'bg-warning-500' },
              { name: 'Binary Search', count: 29, difficulty: 'Intermediate', color: 'bg-pink-500' },
              { name: 'Linked Lists', count: 41, difficulty: 'Beginner Friendly', color: 'bg-indigo-500' },
            ].map((category) => (
              <Link key={category.name} href={`/problems?category=${category.name.toLowerCase().replace(/ & /, '-').replace(/ /g, '-')}`}>
                <Card className="hover:shadow-lg transition-all duration-300 cursor-pointer group">
                  <CardContent className="p-6">
                    <div className="flex items-center justify-between mb-4">
                      <div className={`w-4 h-4 rounded-full ${category.color}`}></div>
                      <Badge variant="outline" className="text-xs">
                        {category.count} problems
                      </Badge>
                    </div>
                    <h3 className="text-lg font-semibold text-slate-900 dark:text-white mb-2 group-hover:text-brand-600 transition-colors">
                      {category.name}
                    </h3>
                    <p className="text-sm text-slate-600 dark:text-slate-400">
                      {category.difficulty}
                    </p>
                  </CardContent>
                </Card>
              </Link>
            ))}
          </div>

          <div className="text-center mt-12">
            <Link href="/problems">
              <Button variant="outline" size="lg" data-testid="button-view-all-problems">
                View All Problems
                <i className="fas fa-arrow-right ml-2"></i>
              </Button>
            </Link>
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="py-20 bg-gradient-to-r from-brand-600 to-brand-700" data-testid="cta-section">
        <div className="max-w-4xl mx-auto text-center px-4 sm:px-6 lg:px-8">
          <h2 className="text-3xl font-bold text-white mb-4">
            Ready to Start Your Coding Journey?
          </h2>
          <p className="text-xl text-brand-100 mb-8">
            Join thousands of developers who are already improving their skills with DSAGrind.
          </p>
          
          <div className="flex flex-col sm:flex-row justify-center gap-4">
            {user ? (
              <Link href="/problems">
                <Button size="lg" className="bg-white text-brand-600 hover:bg-brand-50 text-lg px-8 py-4" data-testid="button-continue-learning">
                  <i className="fas fa-code mr-2"></i>
                  Continue Learning
                </Button>
              </Link>
            ) : (
              <>
                <Button size="lg" className="bg-white text-brand-600 hover:bg-brand-50 text-lg px-8 py-4" data-testid="button-sign-up-free">
                  <i className="fas fa-user-plus mr-2"></i>
                  Sign Up Free
                </Button>
                <Link href="/subscribe">
                  <Button variant="outline" size="lg" className="border-white text-white hover:bg-white hover:text-brand-600 text-lg px-8 py-4" data-testid="button-go-premium">
                    <i className="fas fa-star mr-2"></i>
                    Go Premium
                  </Button>
                </Link>
              </>
            )}
          </div>
        </div>
      </section>
    </div>
  );
}
