(function() {
  function checkStatus(response) {
    if (response.status >= 200 && response.status < 300) {
      return response;
    } else {
      var error = new Error(`${response.status} ${response.statusText}`);
      error.response = response;
      throw error;
    }
  }

  function questionsUrl() {
    const path = window.location.pathname.split('/');
    const slideNumber = path.pop();
    const slug = path.pop();
    const presenter = path.pop();
    return `~/${presenter}/${slug}/${slideNumber}`;
  }

  function load() {
    return fetch(questionsUrl(), { method: 'GET', credentials: 'same-origin' })
      .then(checkStatus)
      .then(res => res.json());
  }

  function submitQuestion(question) {
    const json = JSON.stringify({ text: question });
    const headers = new Headers();
    headers.append('Content-Type', 'application/json');
    headers.append('Content-Length', json.length.toString());
    return fetch(questionsUrl(), { method: 'POST', credentials: 'same-origin', body: json, headers: headers })
      .then(checkStatus);
  }

  const questionFormComponent =
  {
    template: `<form id="question-form" v-on:submit="submit">
  <label for="questionText" class="control-label">Ask a Question:</label>
  <textarea id="questionText" v-model="text"></textarea>
  <button class="btn btn-primary btn-xs" type="submit" :disabled="button.disabled">{{button.text}}</button>
</form>`,
    data: () => ({
      text: '',
      button: {
        text: 'Ask',
        disabled: false
      }
    }),
    methods: {
      reset: function reset(question) {
        this.text = question;
        this.button.text = 'Ask';
        this.button.disabled = false;
      },
      submit: function submit(event) {
        event.preventDefault();
        if ((this.text || '').trim().length === 0) return;
        this.button.text = 'Asking...';
        this.button.disabled = true;
        submitQuestion(this.text)
          .then(() => {
            this.reset('');
          })
          .catch(r => {
            console.log(r);
            this.reset(this.text);
          });
        console.log(this.text);
      }
    }
  };

  const questionCardComponent =
  {
    props: ['from', 'text', 'time', 'id', 'show'],
    template: `
  <div class="card">
    <div class="card-body">
      <h5 class="card-title">{{text}}</h5>
      <h6 class="card-subtitle">{{from}}</h6>
    </div>
  </div>`
  };

  const questionListComponent =
  {
    created() {
      load()
        .then(questions => {
          for (const question of questions) {
            const existing = this.questions.find(q => q.id === question.id);
            if (!existing) {
              this.questions.push(question);
            }
          }
        });
    },
    components: {
      'question-card': questionCardComponent
    },
    data: () => ({
      questions: []
    }),
    template:
      `<div class="question-list"><h4>Questions</h4><question-card v-for="question in questions" v-bind="question"></question-card></div>`
  };

  const vm = new Vue({
    el: '#questions',
    components: {
      'question-list': questionListComponent,
      'question-form': questionFormComponent
    },
    template: `<div><question-list></question-list><question-form></question-form></div>`
  });
})();