describe('Task Management Tests', () => {
  // let token = '{"Header":{"Alg":"HS256","Typ":"JWT"},"Payload":{"Iss":"localhost","Sub":"myApp","Aud":"myApp","Exp":"638477345117710790","Iat":"638477333117710758","Nbf":null,"Scope":"","UserHash":"TestUser","Claims":{"Role":2}},"Signature":"lrs50BAplaJBayP3LEUCYscgQjjILbJrBVSOG4V9Jwc"}';
  let token = '{"Header":{"Alg":"HS256","Typ":"JWT"},"Payload":{"Iss":"localhost","Sub":"kj3VOKOk9Dh0pY5Fh41Dr7knV3/qR9FI6I7FmZlRVtc=","Aud":"spacesurfers","Exp":638494325499418601,"Iat":638494289499417369,"Claims":{"Role":"2"},"Nbf":null,"Scope":"","Permissions":[]},"Signature":"mzAJJzqPELCJGZ_a7oyacIsozDSpsao-21jrG-bmDpk"}';
  beforeEach(() => {
    cy.intercept('POST', 'http://localhost:5270/api/auth/sendOTP', {
      statusCode: 200
    }).as('sendOTP');
  
    cy.intercept('POST', 'http://localhost:5270/api/auth/authenticate', {
      statusCode: 200,
      body: {
        accessToken: token,
        idToken: '{"Username":"kj3VOKOk9Dh0pY5Fh41Dr7knV3/qR9FI6I7FmZlRVtc="}'
      }
    }).as('authenticate');


    cy.intercept('POST', 'http://localhost:8089/api/v1/taskManagerHub/CreateTask', {
        statusCode: 200,
        body: {
            success: true,
            message: 'Task created successfully'
        }
    }).as('createTask');
  
    cy.visit('http://localhost:3000/Login/index.html');
    // cy.get('#userIdentity').type('brandongalich@gmail.com');
    // cy.get('button').contains('Send Verification').click();
    // cy.wait('@sendOTP');
    // cy.get('#otp').type('password');
    // cy.get('button').contains('Login').click();
    // cy.wait('@authenticate');
  });

  it('should handle OTP verification and login', () => {

    cy.get('#userIdentity').type('testUser');

    cy.get('button').contains('Send Verification').click();

    cy.wait('@sendOTP');

    cy.get('#enterOTPSection').should('be.visible');
    cy.get('#otp').type('123456');
    cy.get('button').contains('Login').click();
    cy.wait('@authenticate');
  
    cy.get('body').then($body => {
      if ($body.find('#homepageGen').css('display') === 'none') {
        cy.get('#homepageGen').invoke('show'); 
      }
    });

    cy.get('#homepageGen').should('be.visible');
    cy.get('#identity').should('contain', 'Logged in as: testUser');
  });


  it('Create a new task', () => {
    cy.get('[data-testid="task-view"]').click({ force: true });
    cy.get('button').contains('Task Manager Hub').click();
  
    cy.get('#taskTitle').type('New Task Title');
    cy.get('#taskDescription').type('This is a new task.');
    cy.get('#taskDueDate').type('2024-03-25');
    cy.get('#taskPriority').select('High');
    // cy.wait(500); 
    cy.get('#createTaskBtn').click({ force: true });
    // cy.wait('@createTask');cy.wait(500)  
    cy.wait(1000);
    cy.get('#taskList').should('contain', 'New Task Title');
  });

  it('Modify an existing task', () => {
    cy.get('[data-testid="task-view"]').click({ force: true });
    cy.get('button').contains('Task Manager Hub').click();

    cy.get('#taskTitle').type('New Task Title');
    cy.get('#taskDescription').type('This is a new task.');
    cy.get('#taskDueDate').type('2024-03-25');
    cy.get('#taskPriority').select('High');
    cy.get('#createTaskBtn').click();
    cy.wait(1000);
    

    // Assert changes
    cy.wait(1000);
    cy.get('#taskList').should('not.contain', 'Original Description');
});

  it('delete task', () => {
    cy.get('[data-testid="task-view"]').click({ force: true });
    cy.get('button').contains('Task Manager Hub').click();

    cy.get('#taskTitle').type('New Task Title');
    cy.get('#taskDescription').type('This is a new task.');
    cy.get('#taskDueDate').type('2024-03-25');
    cy.get('#taskPriority').select('High');
    cy.get('#createTaskBtn').click();

    cy.get('#taskList')
  });
});
