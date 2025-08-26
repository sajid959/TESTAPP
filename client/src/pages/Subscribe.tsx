import { useState, useEffect } from 'react';
import { useStripe, useElements, PaymentElement, Elements } from '@stripe/react-stripe-js';
import { loadStripe } from '@stripe/stripe-js';
import { Link } from 'wouter';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { useToast } from '@/hooks/use-toast';
import { useAuth } from '@/hooks/useAuth';
import { apiRequest } from '@/lib/queryClient';

// Load Stripe
const stripePromise = loadStripe(import.meta.env.VITE_STRIPE_PUBLIC_KEY || 'pk_test_default');

function SubscribeForm() {
  const stripe = useStripe();
  const elements = useElements();
  const { toast } = useToast();
  const [isProcessing, setIsProcessing] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!stripe || !elements) {
      return;
    }

    setIsProcessing(true);

    try {
      const { error } = await stripe.confirmPayment({
        elements,
        confirmParams: {
          return_url: `${window.location.origin}/problems`,
        },
      });

      if (error) {
        toast({
          title: 'Payment Failed',
          description: error.message,
          variant: 'destructive',
        });
      } else {
        toast({
          title: 'Payment Successful',
          description: 'Welcome to DSAGrind Premium!',
        });
      }
    } catch (error: any) {
      toast({
        title: 'Payment Error',
        description: error.message || 'An unexpected error occurred',
        variant: 'destructive',
      });
    } finally {
      setIsProcessing(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <PaymentElement />
      <Button
        type="submit"
        disabled={!stripe || isProcessing}
        className="w-full bg-brand-600 hover:bg-brand-700 text-lg py-3"
        data-testid="button-subscribe-submit"
      >
        {isProcessing ? (
          <>
            <i className="fas fa-spinner fa-spin mr-2"></i>
            Processing...
          </>
        ) : (
          <>
            <i className="fas fa-crown mr-2"></i>
            Subscribe to Premium
          </>
        )}
      </Button>
    </form>
  );
}

function SubscribeContent() {
  const { user } = useAuth();
  const { toast } = useToast();
  const [clientSecret, setClientSecret] = useState('');
  const [selectedPlan, setSelectedPlan] = useState<'monthly' | 'annual'>('monthly');
  const [isLoading, setIsLoading] = useState(false);

  const plans = {
    monthly: {
      name: 'Monthly Premium',
      price: '$9.99',
      period: '/month',
      features: [
        'Access to all premium problems',
        'Detailed solution explanations',
        'AI-powered hints and debugging',
        'Advanced progress analytics',
        'Priority customer support',
        'Early access to new features',
      ],
    },
    annual: {
      name: 'Annual Premium',
      price: '$99.99',
      period: '/year',
      originalPrice: '$119.88',
      savings: 'Save $19.89',
      features: [
        'Everything in Monthly Premium',
        '2 months free (17% savings)',
        'Premium-only contests',
        'Exclusive community access',
        'Advanced interview prep tools',
        'Personal coding mentor sessions',
      ],
    },
  };

  const currentPlan = plans[selectedPlan];

  useEffect(() => {
    if (user && !(user as any).isPremium && user?.role !== 'admin') {
      createSubscription();
    }
  }, [user, selectedPlan]);

  const createSubscription = async () => {
    if (!user) {
      toast({
        title: 'Authentication Required',
        description: 'Please sign in to subscribe to premium',
        variant: 'destructive',
      });
      return;
    }

    setIsLoading(true);
    try {
      const response = await apiRequest('POST', '/api/get-or-create-subscription');
      const data = await response.json();
      setClientSecret(data.clientSecret);
    } catch (error: any) {
      toast({
        title: 'Error',
        description: error.message || 'Failed to create subscription',
        variant: 'destructive',
      });
    } finally {
      setIsLoading(false);
    }
  };

  // If user is already premium or admin
  if ((user as any)?.isPremium || user?.role === 'admin') {
    return (
      <div className="min-h-screen bg-slate-50 dark:bg-slate-900 py-20">
        <div className="max-w-2xl mx-auto px-4 sm:px-6 lg:px-8">
          <Card className="text-center">
            <CardContent className="pt-12 pb-8">
              <i className="fas fa-crown text-6xl text-amber-500 mb-6"></i>
              <h2 className="text-2xl font-bold text-slate-900 dark:text-white mb-4">
                You're Already a Premium Member!
              </h2>
              <p className="text-slate-600 dark:text-slate-400 mb-8">
                Enjoy all the premium features and continue your coding journey.
              </p>
              <Link href="/problems">
                <Button className="bg-brand-600 hover:bg-brand-700">
                  <i className="fas fa-code mr-2"></i>
                  Continue Practicing
                </Button>
              </Link>
            </CardContent>
          </Card>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-slate-50 dark:bg-slate-900 py-20">
      <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8">
        {/* Header */}
        <div className="text-center mb-16">
          <h1 className="text-4xl font-bold text-slate-900 dark:text-white mb-4">
            <i className="fas fa-crown text-amber-500 mr-3"></i>
            Upgrade to Premium
          </h1>
          <p className="text-xl text-slate-600 dark:text-slate-400 max-w-3xl mx-auto">
            Unlock advanced features, premium problems, and AI-powered insights to accelerate your coding journey.
          </p>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
          {/* Plan Selection */}
          <div className="space-y-6">
            <h2 className="text-2xl font-semibold text-slate-900 dark:text-white">
              Choose Your Plan
            </h2>

            {/* Plan Toggle */}
            <div className="flex bg-slate-200 dark:bg-slate-800 rounded-lg p-1">
              <button
                onClick={() => setSelectedPlan('monthly')}
                className={`flex-1 py-2 px-4 rounded-md text-sm font-medium transition-colors ${
                  selectedPlan === 'monthly'
                    ? 'bg-white dark:bg-slate-900 text-slate-900 dark:text-white shadow-sm'
                    : 'text-slate-600 dark:text-slate-400'
                }`}
                data-testid="button-monthly-plan"
              >
                Monthly
              </button>
              <button
                onClick={() => setSelectedPlan('annual')}
                className={`flex-1 py-2 px-4 rounded-md text-sm font-medium transition-colors ${
                  selectedPlan === 'annual'
                    ? 'bg-white dark:bg-slate-900 text-slate-900 dark:text-white shadow-sm'
                    : 'text-slate-600 dark:text-slate-400'
                }`}
                data-testid="button-annual-plan"
              >
                <span>Annual</span>
                <Badge className="ml-2 bg-success-500 text-white">Save 17%</Badge>
              </button>
            </div>

            {/* Selected Plan Details */}
            <Card className="border-2 border-brand-200 dark:border-brand-800">
              <CardHeader>
                <div className="flex items-center justify-between">
                  <CardTitle className="text-xl text-slate-900 dark:text-white">
                    {currentPlan.name}
                  </CardTitle>
                  {selectedPlan === 'annual' && (
                    <Badge className="bg-success-500 text-white">Most Popular</Badge>
                  )}
                </div>
                <div className="flex items-baseline space-x-2">
                  <span className="text-3xl font-bold text-brand-600">
                    {currentPlan.price}
                  </span>
                  <span className="text-slate-600 dark:text-slate-400">
                    {currentPlan.period}
                  </span>
                  {(currentPlan as any).originalPrice && (
                    <span className="text-sm text-slate-500 line-through">
                      {(currentPlan as any).originalPrice}
                    </span>
                  )}
                </div>
                {(currentPlan as any).savings && (
                  <p className="text-sm text-success-600 font-medium">
                    {(currentPlan as any).savings}
                  </p>
                )}
              </CardHeader>
              <CardContent>
                <ul className="space-y-3">
                  {currentPlan.features.map((feature, index) => (
                    <li key={index} className="flex items-center">
                      <i className="fas fa-check text-success-600 mr-3"></i>
                      <span className="text-slate-700 dark:text-slate-300">{feature}</span>
                    </li>
                  ))}
                </ul>
              </CardContent>
            </Card>

            {/* Feature Comparison */}
            <Card>
              <CardHeader>
                <CardTitle className="text-lg">Free vs Premium</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  {[
                    { feature: 'Basic Problems', free: '✓', premium: '✓' },
                    { feature: 'Premium Problems', free: '✗', premium: '✓' },
                    { feature: 'AI Hints', free: 'Limited', premium: 'Unlimited' },
                    { feature: 'Solution Explanations', free: '✗', premium: '✓' },
                    { feature: 'Progress Analytics', free: 'Basic', premium: 'Advanced' },
                    { feature: 'Priority Support', free: '✗', premium: '✓' },
                  ].map((item, index) => (
                    <div key={index} className="grid grid-cols-3 gap-4 py-2">
                      <span className="text-sm font-medium text-slate-900 dark:text-white">
                        {item.feature}
                      </span>
                      <span className="text-sm text-center text-slate-600 dark:text-slate-400">
                        {item.free}
                      </span>
                      <span className="text-sm text-center text-brand-600 font-medium">
                        {item.premium}
                      </span>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          </div>

          {/* Payment Form */}
          <div className="space-y-6">
            <h2 className="text-2xl font-semibold text-slate-900 dark:text-white">
              Payment Details
            </h2>

            <Card>
              <CardHeader>
                <CardTitle>Complete Your Subscription</CardTitle>
              </CardHeader>
              <CardContent>
                {!user ? (
                  <div className="text-center py-8">
                    <i className="fas fa-user-circle text-4xl text-slate-300 dark:text-slate-600 mb-4"></i>
                    <h3 className="text-lg font-semibold text-slate-900 dark:text-white mb-2">
                      Sign in Required
                    </h3>
                    <p className="text-slate-600 dark:text-slate-400 mb-4">
                      Please sign in to your account to subscribe to premium.
                    </p>
                    <Button className="bg-brand-600 hover:bg-brand-700">
                      Sign In
                    </Button>
                  </div>
                ) : isLoading ? (
                  <div className="text-center py-8">
                    <div className="animate-spin w-8 h-8 border-4 border-brand-600 border-t-transparent rounded-full mx-auto mb-4"></div>
                    <p className="text-slate-600 dark:text-slate-400">
                      Setting up your subscription...
                    </p>
                  </div>
                ) : clientSecret ? (
                  <Elements stripe={stripePromise} options={{ clientSecret }}>
                    <SubscribeForm />
                  </Elements>
                ) : (
                  <div className="text-center py-8">
                    <Button
                      onClick={createSubscription}
                      className="bg-brand-600 hover:bg-brand-700"
                      data-testid="button-create-subscription"
                    >
                      <i className="fas fa-credit-card mr-2"></i>
                      Set Up Payment
                    </Button>
                  </div>
                )}
              </CardContent>
            </Card>

            {/* Security Notice */}
            <Card className="bg-blue-50 dark:bg-blue-950 border-blue-200 dark:border-blue-800">
              <CardContent className="p-4">
                <div className="flex items-start space-x-3">
                  <i className="fas fa-shield-alt text-blue-600 mt-1"></i>
                  <div>
                    <h4 className="font-semibold text-blue-900 dark:text-blue-100 mb-1">
                      Secure Payment
                    </h4>
                    <p className="text-sm text-blue-800 dark:text-blue-200">
                      Your payment information is processed securely through Stripe. We never store your card details.
                    </p>
                  </div>
                </div>
              </CardContent>
            </Card>

            {/* Money-back Guarantee */}
            <Card className="bg-success-50 dark:bg-success-950 border-success-200 dark:border-success-800">
              <CardContent className="p-4">
                <div className="flex items-start space-x-3">
                  <i className="fas fa-money-bill-wave text-success-600 mt-1"></i>
                  <div>
                    <h4 className="font-semibold text-success-900 dark:text-success-100 mb-1">
                      30-Day Money-Back Guarantee
                    </h4>
                    <p className="text-sm text-success-800 dark:text-success-200">
                      Not satisfied? Get a full refund within 30 days, no questions asked.
                    </p>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>
        </div>

        {/* FAQ Section */}
        <div className="mt-20">
          <h2 className="text-2xl font-semibold text-slate-900 dark:text-white text-center mb-8">
            Frequently Asked Questions
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {[
              {
                question: 'Can I cancel my subscription anytime?',
                answer: 'Yes, you can cancel your subscription at any time from your account settings. Your premium access will continue until the end of your billing period.',
              },
              {
                question: 'Do you offer student discounts?',
                answer: 'Yes! We offer a 50% student discount. Contact our support team with your student email for verification.',
              },
              {
                question: 'What payment methods do you accept?',
                answer: 'We accept all major credit cards, debit cards, and PayPal through our secure Stripe payment processing.',
              },
              {
                question: 'Is there a free trial available?',
                answer: 'Yes, new users get 7 days of premium access free. No credit card required to start your trial.',
              },
            ].map((faq, index) => (
              <Card key={index}>
                <CardContent className="p-6">
                  <h3 className="font-semibold text-slate-900 dark:text-white mb-2">
                    {faq.question}
                  </h3>
                  <p className="text-slate-600 dark:text-slate-400 text-sm">
                    {faq.answer}
                  </p>
                </CardContent>
              </Card>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}

export default function Subscribe() {
  return <SubscribeContent />;
}
