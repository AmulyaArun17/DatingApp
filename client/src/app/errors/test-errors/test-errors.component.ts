import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-test-errors',
  templateUrl: './test-errors.component.html',
  styleUrls: ['./test-errors.component.css']
})
export class TestErrorsComponent implements OnInit {
  baseUrl = 'https://localhost:5001/api/';
  user = JSON.parse(localStorage.getItem('user'));
  header = new HttpHeaders().set(
    "Authorization",
     "Bearer " + this.user.token
  );
  validationErrors: string[] =[];
  constructor(private http: HttpClient) { }

  ngOnInit(): void {
  }

  get404Error() {
    this.http.get(this.baseUrl + 'buggy/not-found',{headers: this.header}).subscribe(response => { console.log(response); },
      error => { console.log(error); }
    );
  }

  get400Error() {
    this.http.get(this.baseUrl + 'buggy/bad-request',{headers: this.header}).subscribe(response => { console.log(response); },
      error => { console.log(error); }
    );
  }

  get500Error() {
    this.http.get(this.baseUrl + 'buggy/server-error',{headers: this.header}).subscribe(response => { console.log(response); },
      error => { console.log(error); }
    );
  }

  get401Error() {
    this.http.get(this.baseUrl + 'buggy/auth',{headers: this.header}).subscribe(response => { console.log(response); },
      error => { console.log(error); }
    );
  }

  get400ValidationError() {
    this.http.post(this.baseUrl + 'account/register',{},{headers: this.header}).subscribe(response => { console.log(response); },
      error => { console.log(error);
      this.validationErrors = error }
    );
  }
}
