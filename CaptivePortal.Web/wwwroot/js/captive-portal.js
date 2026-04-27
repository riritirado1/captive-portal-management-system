// Captive Portal JavaScript Functionality

// Initialize on DOM load
document.addEventListener('DOMContentLoaded', function() {
    initializeCaptivePortal();
});

function initializeCaptivePortal() {
    // Email validation
    const emailInput = document.getElementById('email');
    const form = document.querySelector('.portal-form');
    const connectButton = document.querySelector('.btn-connect');
    
    if (emailInput) {
        emailInput.addEventListener('input', validateEmailRealTime);
        emailInput.addEventListener('blur', validateEmailOnBlur);
    }
    
    if (form) {
        form.addEventListener('submit', handleFormSubmit);
    }
    
    // Auto-focus email input
    if (emailInput) {
        emailInput.focus();
    }
    
    // Initialize terms modal
    initializeTermsModal();
}

// Real-time email validation
function validateEmailRealTime() {
    const emailInput = document.getElementById('email');
    const email = emailInput.value;
    
    // Basic format validation
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    
    if (email.length > 0) {
        if (emailRegex.test(email)) {
            emailInput.classList.remove('input-validation-error');
            emailInput.classList.add('is-valid');
        } else {
            emailInput.classList.add('input-validation-error');
            emailInput.classList.remove('is-valid');
        }
    } else {
        emailInput.classList.remove('input-validation-error', 'is-valid');
    }
}

// Email validation on blur (focus lost)
function validateEmailOnBlur() {
    const emailInput = document.getElementById('email');
    const email = emailInput.value.trim();
    
    if (email.length > 0) {
        validateEmailDomain(email);
    }
}

// Enhanced email domain validation
function validateEmailDomain(email) {
    const emailInput = document.getElementById('email');
    
    // List of common disposable email domains to block
    const disposableDomains = [
        '10minutemail.com',
        'tempmail.org',
        'guerrillamail.com',
        'mailinator.com',
        'throwaway.email'
    ];
    
    const domain = email.split('@')[1]?.toLowerCase();
    
    if (domain && disposableDomains.includes(domain)) {
        showEmailError('Please use a permanent email address.');
        emailInput.classList.add('input-validation-error');
        return false;
    }
    
    return true;
}

// Show email validation error
function showEmailError(message) {
    let errorElement = document.querySelector('.email-error');
    
    if (!errorElement) {
        errorElement = document.createElement('div');
        errorElement.className = 'field-validation-error email-error';
        const emailInput = document.getElementById('email');
        emailInput.parentNode.appendChild(errorElement);
    }
    
    errorElement.textContent = message;
}

// Handle form submission
function handleFormSubmit(event) {
    const form = event.target;
    const emailInput = document.getElementById('email');
    const termsCheckbox = document.getElementById('acceptTerms');
    const connectButton = document.querySelector('.btn-connect');
    
    // Validate email
    if (!validateEmailDomain(emailInput.value)) {
        event.preventDefault();
        return false;
    }
    
    // Validate terms acceptance
    if (!termsCheckbox.checked) {
        event.preventDefault();
        alert('Please accept the Terms of Service to continue.');
        termsCheckbox.focus();
        return false;
    }
    
    // Show loading state
    showLoadingState(connectButton);
    
    // Allow form submission to proceed
    return true;
}

// Show loading state on submit button
function showLoadingState(button) {
    const originalText = button.innerHTML;
    button.innerHTML = '<i class="bi bi-spinner-border spinning"></i> Connecting...';
    button.disabled = true;
    
    // Add spinning animation
    const style = document.createElement('style');
    style.textContent = `
        .spinning {
            animation: spin 1s linear infinite;
        }
        @keyframes spin {
            from { transform: rotate(0deg); }
            to { transform: rotate(360deg); }
        }
    `;
    document.head.appendChild(style);
}

// Initialize terms modal
function initializeTermsModal() {
    // Create modal instance when needed
    window.showTerms = function() {
        const modal = new bootstrap.Modal(document.getElementById('termsModal'));
        modal.show();
    };
}

// Track advertisement clicks
function trackAdClick(adId) {
    if (adId) {
        // Send click tracking request
        fetch('/api/ads/track-click', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ advertisementId: adId })
        }).catch(error => {
            console.log('Ad click tracking failed:', error);
        });
    }
}

// Auto-complete email domains
function setupEmailAutoComplete() {
    const emailInput = document.getElementById('email');
    
    if (emailInput) {
        const commonDomains = [
            'gmail.com',
            'yahoo.com',
            'hotmail.com',
            'outlook.com',
            'wtamu.edu',
            'student.wtamu.edu'
        ];
        
        emailInput.addEventListener('keyup', function() {
            const value = this.value;
            const atIndex = value.indexOf('@');
            
            if (atIndex > 0 && atIndex === value.length - 1) {
                // User just typed @, show suggestions
                showEmailSuggestions(value, commonDomains);
            }
        });
    }
}

// Show email domain suggestions
function showEmailSuggestions(partial, domains) {
    // Remove existing suggestions
    const existingSuggestions = document.querySelector('.email-suggestions');
    if (existingSuggestions) {
        existingSuggestions.remove();
    }
    
    // Create suggestions container
    const suggestionsContainer = document.createElement('div');
    suggestionsContainer.className = 'email-suggestions';
    suggestionsContainer.style.cssText = `
        position: absolute;
        background: white;
        border: 1px solid #ddd;
        border-radius: 4px;
        box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        z-index: 1000;
        max-height: 200px;
        overflow-y: auto;
        width: 100%;
        margin-top: 2px;
    `;
    
    // Add suggestions
    domains.forEach(domain => {
        const suggestion = document.createElement('div');
        suggestion.textContent = partial + domain;
        suggestion.style.cssText = `
            padding: 10px;
            cursor: pointer;
            border-bottom: 1px solid #eee;
        `;
        
        suggestion.addEventListener('click', function() {
            document.getElementById('email').value = this.textContent;
            suggestionsContainer.remove();
        });
        
        suggestionsContainer.appendChild(suggestion);
    });
    
    // Insert suggestions after email input
    const emailInput = document.getElementById('email');
    emailInput.parentNode.style.position = 'relative';
    emailInput.parentNode.appendChild(suggestionsContainer);
    
    // Remove suggestions when clicking elsewhere
    setTimeout(() => {
        document.addEventListener('click', function(e) {
            if (!suggestionsContainer.contains(e.target) && e.target !== emailInput) {
                suggestionsContainer.remove();
            }
        });
    }, 100);
}

// Connection status check (for future use)
function checkConnectionStatus() {
    return fetch('/api/connection/status')
        .then(response => response.json())
        .then(data => data.isConnected)
        .catch(() => false);
}

// Initialize auto-complete on load
document.addEventListener('DOMContentLoaded', setupEmailAutoComplete);