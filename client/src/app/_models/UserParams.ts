import { User } from 'src/app/_models/User';
export class UserParams {
  gender: string;
  minAge: number;
  maxAge: number;
  pageNumber: number;
  pageSize: number;
  orderBy: string;
  constructor(user: User) {
    this.gender = user.gender === 'female' ? 'male' : 'female';
    this.minAge = 18;
    this.maxAge = 99;
    this.pageNumber = 1;
    this.pageSize = 5;
    this.orderBy = 'lastActive';
  }
}
