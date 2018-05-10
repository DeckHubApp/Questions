(function(DeckHub, currentScript) {

  function _urlPrefix(src) {
    var url = new URL(src, document.location.href);
    let parts = url.pathname.split('/').filter(s => !!s).slice(0, -1);
    if (parts.length === 0) {
      return '';
    }
    return `/${parts.join('/')}`;
  }

  const urlPrefix = _urlPrefix(currentScript.src);

  function checkStatus(response) {
    if (response.status >= 200 && response.status < 300) {
      return response;
    } else {
      var error = new Error(`${response.status} ${response.statusText}`);
      error.response = response;
      throw error;
    }
  }

  function pagePath() {
    return DeckHub.DEV_PATH || window.location.pathname;
  }

  function questionsUrl() {
    const path = pagePath().split('/').filter(s => !!s);
    let slideNumber = path.pop();
    const slug = path.pop();
    const presenter = path.pop();
    const place = path.pop();

    const show = DeckHub.show || `${place}/${presenter}/${slug}`;
    if (DeckHub.slide === 'all') {
      return `${urlPrefix}/${show}`;
    }
    if (DeckHub.slide || DeckHub.slide === 0) {
      slideNumber = DeckHub.slide;
    }

    return `${urlPrefix}/${show}/${slideNumber}`;
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

  const questionFormComponent = {
    template: `<form id="question-form" v-on:submit="submit" class="m-2">
<div class="input-group">
  <input id="questionText" v-model="text" placeholder="Ask a question" class="form-control" />
  <div class="input-group-append">
  <button class="btn btn-secondary" type="submit" :disabled="button.disabled">{{button.text}}</button>
</div>
</div>
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
      console.log(this.questions);
    },
    props: ['questions'],
    components: {
      'question-card': questionCardComponent
    },
    template:
      `
<div class="question-list m-2">
  <h6>All questions:</h6>
  <question-card v-for="question in questions" v-bind="question"></question-card>
</div>`
  };

  const vm = new Vue({
    el: '#questions',
    components: {
      'question-list': questionListComponent,
      'question-form': questionFormComponent
    },
    data: {
      questions: [],
      userIsAuthenticated: false
    },
    methods: {
      add: function add(question) {
        const existing = this.questions.find(q => q.id === question.id);
        if (!existing) {
          this.questions.unshift(question);
        }
      }
    },
    created() {
      load()
        .then(q => {
          this.userIsAuthenticated = !!q.userIsAuthenticated;
          for (const question of q.questions) {
            this.add(question);
          }
        });
      DeckHub.Hub.subject('question').subscribe(this.add);
    },
    template: `<div><question-form v-if="userIsAuthenticated"></question-form><question-list v-bind:questions="questions"></question-list></div>`
  });
})(window.DeckHub || (window.DeckHub = {}), document.currentScript);